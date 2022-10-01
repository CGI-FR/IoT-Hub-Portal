// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Models.v10;
    using Domain.Repositories;
    using Domain.Exceptions;
    using Managers;
    using Infrastructure;
    using AzureIoTHub.Portal.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Domain;
    using Mappers;

    public class DeviceService : DeviceServiceBase<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
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
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public override async Task<DeviceDetails> GetDevice(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);

            if (deviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<DeviceDetails>(deviceEntity);

            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);

            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        public override async Task<DeviceDetails> CreateDevice(DeviceDetails device)
        {
            _ = base.CreateDevice(device);

            try
            {
                var deviceEntity = this.mapper.Map<Device>(device);

                await this.deviceRepository.InsertAsync(deviceEntity);
                await this.unitOfWork.SaveAsync();

                return device;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create the device {device.DeviceName}", e);
            }
        }

        public override async Task<DeviceDetails> UpdateDevice(DeviceDetails device)
        {
            _ = await base.UpdateDevice(device);

            try
            {
                var deviceEntity = await this.deviceRepository.GetByIdAsync(device.DeviceID);

                if (deviceEntity == null)
                {
                    throw new ResourceNotFoundException($"The device {device.DeviceID} doesn't exist");
                }

                _ = this.mapper.Map(device, deviceEntity);

                this.deviceRepository.Update(deviceEntity);
                await this.unitOfWork.SaveAsync();

                return device;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to update the device {device.DeviceName}", e);
            }
        }

        public override async Task DeleteDevice(string deviceId)
        {
            await base.DeleteDevice(deviceId);

            try
            {
                var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);

                if (deviceEntity == null)
                {
                    return;
                }

                this.deviceRepository.Delete(deviceId);

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device {deviceId}", e);
            }
        }
    }
}
