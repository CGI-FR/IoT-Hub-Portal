// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.v10.LoRaWAN;
    using AutoMapper;
    using Domain.Entities;
    using Domain.Repositories;
    using Managers;
    using Infrastructure;
    using Domain.Exceptions;
    using Domain;
    using Models.v10;
    using Mappers;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;
    using System;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using Azure.Messaging.EventHubs;
    using AzureIoTHub.Portal.Shared.Models.v10;

    public class LoRaWanDeviceService : DeviceServiceBase<LoRaDeviceDetails>
    {
        private readonly ILogger<LoRaWanDeviceService> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly ILoRaDeviceTelemetryRepository deviceTelemetryRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaWanDeviceService(
            ILogger<LoRaWanDeviceService> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILorawanDeviceRepository lorawanDeviceRepository,
            ILoRaDeviceTelemetryRepository deviceTelemetryRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            PortalDbContext portalDbContext,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper)
            : base(portalDbContext, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceTelemetryRepository = deviceTelemetryRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public override async Task<LoRaDeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<LoRaDeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        protected override async Task<LoRaDeviceDetails> CreateDeviceInDatabase(LoRaDeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<LorawanDevice>(device);

            await this.lorawanDeviceRepository.InsertAsync(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task<LoRaDeviceDetails> UpdateDeviceInDatabase(LoRaDeviceDetails device)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device {device.DeviceID} doesn't exist");
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            _ = this.mapper.Map(device, deviceEntity);

            this.lorawanDeviceRepository.Update(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags);

            if (deviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            this.lorawanDeviceRepository.Delete(deviceId);

            await this.unitOfWork.SaveAsync();
        }

        public override async Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Telemetry);

            return deviceEntity == null ? new List<LoRaDeviceTelemetryDto>() : this.mapper.Map<ICollection<LoRaDeviceTelemetry>, IEnumerable<LoRaDeviceTelemetryDto>>(deviceEntity.Telemetry);
        }

        public override async Task ProcessTelemetryEvent(EventData eventMessage)
        {
            try
            {
                if (eventMessage == null) return;

                LoRaDeviceTelemetry deviceTelemetry;

                try
                {
                    deviceTelemetry = new LoRaDeviceTelemetry
                    {
                        Id = eventMessage.SequenceNumber.ToString(CultureInfo.InvariantCulture),
                        EnqueuedTime = eventMessage.EnqueuedTime.UtcDateTime,
                        Telemetry = eventMessage.EventBody.ToObjectFromJson<LoRaTelemetry>()
                    };
                }
                catch (JsonException)
                {
                    this.logger.LogWarning($"Unable to deserialize the event message with id {eventMessage.SequenceNumber} as device telemetry");
                    return;
                }

                var loRaWanDevice = await this.lorawanDeviceRepository.GetByIdAsync(deviceTelemetry.Telemetry.DeviceEUI, device => device.Telemetry);

                if (loRaWanDevice == null)
                {
                    return;
                }

                if (loRaWanDevice.Telemetry.Any(telemetry => telemetry.Id.Equals(deviceTelemetry.Id, StringComparison.Ordinal)))
                {
                    return;
                }

                loRaWanDevice.Telemetry.Add(deviceTelemetry);

                await this.unitOfWork.SaveAsync();

                await KeepOnlyLatestHundredTelemetry(loRaWanDevice);
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, $"Unable to store the LoRa telemetry message with sequence number {eventMessage.SequenceNumber}");
            }

            return;
        }

        private async Task KeepOnlyLatestHundredTelemetry(LorawanDevice loRaWanDevice)
        {
            if (loRaWanDevice.Telemetry.Count <= 100) return;

            loRaWanDevice.Telemetry
                .OrderByDescending(telemetry => telemetry.EnqueuedTime)
                .Skip(100)
                .ToList()
                .ForEach(telemetry =>
                {
                    this.deviceTelemetryRepository.Delete(telemetry.Id);
                });

            await this.unitOfWork.SaveAsync();
        }
    }
}
