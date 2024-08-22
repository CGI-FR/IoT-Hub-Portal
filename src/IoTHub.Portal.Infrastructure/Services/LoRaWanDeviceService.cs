// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;
    using static IoTHub.Portal.Infrastructure.ConnectionAuthMethod;

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
            : base(portalDbContext, mapper, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper, logger)
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

                if (!eventMessage.SystemProperties.TryGetValue("iothub-connection-auth-method", out var authMethod))
                {
                    this.logger.LogWarning($"Unable read 'iothub-connection-auth-method' property of the message. Please verify that the event is comming from an IoT Device.");
                    return;
                }

                var eventAuthMethod = JsonSerializer.Deserialize<ConnectionAuthMethod>(authMethod.ToString(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                        {
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                });

                if (eventAuthMethod.Scope != ConnectionAuthScope.Device)
                {
                    this.logger.LogTrace($"Event wasn't issued by a device. Skipping this event.");
                    return;
                }

                if (!eventMessage.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId))
                {
                    this.logger.LogWarning($"Unable read 'iothub-connection-device-id' property of the message. Please verify that the event is comming from an IoT Device.");
                    return;
                }

                var loRaWanDevice = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, device => device.Telemetry);

                if (loRaWanDevice == null)
                {
                    return;
                }

                deviceTelemetry = new LoRaDeviceTelemetry
                {
                    Id = eventMessage.SequenceNumber.ToString(CultureInfo.InvariantCulture),
                    EnqueuedTime = eventMessage.EnqueuedTime.UtcDateTime,
                    Telemetry = eventMessage.EventBody.ToObjectFromJson<LoRaTelemetry>()
                };

                if (loRaWanDevice.Telemetry.Any(telemetry => telemetry.Id.Equals(deviceTelemetry.Id, StringComparison.Ordinal)))
                {
                    return;
                }

                loRaWanDevice.Telemetry.Add(deviceTelemetry);

                KeepOnlyLatestTelemetry(loRaWanDevice);

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, $"Unable to store the LoRa telemetry message with sequence number {eventMessage.SequenceNumber}");
            }

            return;
        }

        private void KeepOnlyLatestTelemetry(LorawanDevice loRaWanDevice, int numberOfMessages = 100)
        {
            if (loRaWanDevice.Telemetry.Count <= numberOfMessages) return;

            loRaWanDevice.Telemetry
                .OrderByDescending(telemetry => telemetry.EnqueuedTime)
                .Skip(numberOfMessages)
                .ToList()
                .ForEach(telemetry =>
                {
                    this.deviceTelemetryRepository.Delete(telemetry.Id);
                    _ = loRaWanDevice.Telemetry.Remove(telemetry);
                });
        }
    }
}
