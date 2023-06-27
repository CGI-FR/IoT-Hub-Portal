// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Common.Services
{
    using AutoMapper;
    using Domain;
    using Domain.Exceptions;
    using Domain.Repositories;
    using global::Azure.Messaging.EventHubs;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DeviceService : DeviceServiceBase<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            PortalDbContext portalDbContext,
            ILogger<DeviceService> logger)
            : base(portalDbContext, mapper, externalDevicesService, deviceTagService, deviceModelImageManager, logger)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.labelRepository = labelRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }


        public override async Task<DeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await deviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            var deviceDto = mapper.Map<DeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        public override async Task<bool> CheckIfDeviceExists(string deviceId)
        {
            var deviceEntity = await deviceRepository.GetByIdAsync(deviceId);
            return deviceEntity != null;
        }

        protected override async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = mapper.Map<Device>(device);

            await deviceRepository.InsertAsync(deviceEntity);
            await unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task<DeviceDetails> UpdateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = await deviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device {device.DeviceID} doesn't exist");
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

            deviceRepository.Update(deviceEntity);
            await unitOfWork.SaveAsync();

            return device;
        }


        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await deviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

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

            deviceRepository.Delete(deviceId);

            await unitOfWork.SaveAsync();
        }

        public override async Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            return await Task.Run(Array.Empty<LoRaDeviceTelemetryDto>);
        }

        public override Task ProcessTelemetryEvent(EventData eventMessage)
        {
            return Task.CompletedTask;
        }
    }
}
