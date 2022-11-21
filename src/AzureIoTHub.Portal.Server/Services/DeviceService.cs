// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using Domain;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Infrastructure;
    using Managers;
    using Mappers;
    using Models.v10;
    using Shared.Models.v1._0;

    public class DeviceService : DeviceServiceBase<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper,
            PortalDbContext portalDbContext)
            : base(portalDbContext, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }


        public override async Task<DeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId, d => d.Tags);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<DeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
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
            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.DeviceID, d => d.Tags);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device {device.DeviceID} doesn't exist");
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            _ = this.mapper.Map(device, deviceEntity);

            this.deviceRepository.Update(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }


        protected override async Task DeleteDeviceInDatabase(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId, d => d.Tags);

            if (deviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in deviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            this.deviceRepository.Delete(deviceId);

            await this.unitOfWork.SaveAsync();
        }

        public override async Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            return await Task.Run(Array.Empty<LoRaDeviceTelemetryDto>);
        }
    }
}
