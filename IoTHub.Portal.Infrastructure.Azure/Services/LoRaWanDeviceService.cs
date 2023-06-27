// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Services
{
    using AutoMapper;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using global::Azure.Messaging.EventHubs;
    using Infrastructure;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Infrastructure.Common;
    using IoTHub.Portal.Infrastructure.Common.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
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
            var deviceEntity = await lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device with id {deviceId} doesn't exist");
            }

            var deviceDto = mapper.Map<LoRaDeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        public override async Task<bool> CheckIfDeviceExists(string deviceId)
        {
            var deviceEntity = await lorawanDeviceRepository.GetByIdAsync(deviceId);
            return deviceEntity != null;
        }

        protected override async Task<LoRaDeviceDetails> CreateDeviceInDatabase(LoRaDeviceDetails device)
        {
            var deviceEntity = mapper.Map<LorawanDevice>(device);

            await lorawanDeviceRepository.InsertAsync(deviceEntity);
            await unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task<LoRaDeviceDetails> UpdateDeviceInDatabase(LoRaDeviceDetails device)
        {
            var deviceEntity = await lorawanDeviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The LoRaWAN device {device.DeviceID} doesn't exist");
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in deviceEntity.Labels)
            {
                labelRepository.Delete(labelEntity.Id);
            }

            _ = mapper.Map(device, deviceEntity);

            lorawanDeviceRepository.Update(deviceEntity);
            await unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in deviceEntity.Labels)
            {
                labelRepository.Delete(labelEntity.Id);
            }

            lorawanDeviceRepository.Delete(deviceId);

            await unitOfWork.SaveAsync();
        }

        public override async Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            var deviceEntity = await lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Telemetry);

            return deviceEntity == null ? new List<LoRaDeviceTelemetryDto>() : mapper.Map<ICollection<LoRaDeviceTelemetry>, IEnumerable<LoRaDeviceTelemetryDto>>(deviceEntity.Telemetry);
        }

        public override async Task ProcessTelemetryEvent(EventData eventMessage)
        {
            try
            {
                if (eventMessage == null) return;

                LoRaDeviceTelemetry deviceTelemetry;

                if (!eventMessage.SystemProperties.TryGetValue("iothub-connection-auth-method", out var authMethod))
                {
                    logger.LogWarning($"Unable read 'iothub-connection-auth-method' property of the message. Please verify that the event is comming from an IoT Device.");
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
                    logger.LogTrace($"Event wasn't issued by a device. Skipping this event.");
                    return;
                }

                if (!eventMessage.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId))
                {
                    logger.LogWarning($"Unable read 'iothub-connection-device-id' property of the message. Please verify that the event is comming from an IoT Device.");
                    return;
                }

                var loRaWanDevice = await lorawanDeviceRepository.GetByIdAsync(deviceId, device => device.Telemetry);

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

                await unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, $"Unable to store the LoRa telemetry message with sequence number {eventMessage.SequenceNumber}");
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
                    deviceTelemetryRepository.Delete(telemetry.Id);
                    _ = loRaWanDevice.Telemetry.Remove(telemetry);
                });
        }
    }
}
