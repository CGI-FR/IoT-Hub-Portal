// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Mappers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Domain;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Infrastructure;
    using Models.v10;

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
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper,
            PortalDbContext portalDbContext)
            : base(portalDbContext, mapper, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
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
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<DeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        public override async Task<bool> CheckIfDeviceExists(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);
            return deviceEntity != null;
        }

        protected override async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<Device>(device);

            await this.deviceRepository.InsertAsync(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        protected override async Task<DeviceDetails> UpdateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags, d => d.Labels);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device {device.DeviceID} doesn't exist");
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

            this.deviceRepository.Update(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }


        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

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

            this.deviceRepository.Delete(deviceId);

            await this.unitOfWork.SaveAsync();
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
