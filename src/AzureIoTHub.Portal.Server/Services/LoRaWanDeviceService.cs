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
    using Shared.Models.v1._0;

    public class LoRaWanDeviceService : DeviceServiceBase<LoRaDeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaWanDeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            ILorawanDeviceRepository lorawanDeviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            PortalDbContext portalDbContext,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper)
            : base(portalDbContext, externalDevicesService, deviceTagService, deviceModelImageManager, deviceTwinMapper)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
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

        public override async Task<IEnumerable<DeviceTelemetryDto>> GetDeviceTelemetries(string deviceId)
        {
            var deviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(deviceId, d => d.Telemetries);

            return deviceEntity == null ? new List<DeviceTelemetryDto>() : this.mapper.Map<ICollection<DeviceTelemetry>, IEnumerable<DeviceTelemetryDto>>(deviceEntity.Telemetries);
        }
    }
}
