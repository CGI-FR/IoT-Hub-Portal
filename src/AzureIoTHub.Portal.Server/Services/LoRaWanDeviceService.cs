// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Mappers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Models.v10.LoRaWAN;

    public class LoRaWanDeviceService : DeviceServiceBase<LoRaDeviceDetails>
    {
        private readonly ILogger<LoRaWanDeviceService> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly ILoRaDeviceTelemetryRepository deviceTelemetryRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaWanDeviceService(
            ILogger<LoRaWanDeviceService> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILorawanDeviceRepository lorawanDeviceRepository,
            ILoRaDeviceTelemetryRepository deviceTelemetryRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            PortalDbContext portalDbContext,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper)
            : base(portalDbContext, mapper, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceTelemetryRepository = deviceTelemetryRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.labelRepository = labelRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public override async Task<LoRaDeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<LoRaDeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        public override async Task<bool> CheckIfDeviceExists(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId);
            return deviceEntity != null;
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
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device {device.DeviceID} doesn't exist");
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in deviceEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            _ = this.mapper.Map(device, deviceEntity);

            this.lorawanDeviceRepository.Update(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in deviceEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
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
