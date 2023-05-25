// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using Models.v10;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain;
    using Amazon.IoT.Model;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using Amazon.IotData.Model;
    using AzureIoTHub.Portal.Application.Managers;
    using Infrastructure;
    using Device = Domain.Entities.Device;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Azure.Messaging.EventHubs;

    public class AWSDeviceService : DeviceService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IAWSExternalDeviceService externalDevicesService;

        public AWSDeviceService(PortalDbContext portalDbContext,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IAWSExternalDeviceService externalDevicesService,
            ILogger<AWSDeviceService> logger)
            : base(mapper, unitOfWork, deviceRepository, deviceTagValueRepository, labelRepository, null!, deviceTagService, deviceModelImageManager, null!, portalDbContext, logger)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.externalDevicesService = externalDevicesService;
        }

        public override async Task<bool> CheckIfDeviceExists(string deviceId)
        {
            var deviceEntity = await this.deviceRepository.GetByIdAsync(deviceId);
            return deviceEntity != null;
        }

        public override async Task<DeviceDetails> CreateDevice(DeviceDetails device)
        {
            //Create Thing
            var createThingRequest = this.mapper.Map<CreateThingRequest>(device);
            var response = await this.externalDevicesService.CreateDevice(createThingRequest);
            device.DeviceID = response.ThingId;

            //Create Thing Shadow
            var shadowRequest = this.mapper.Map<UpdateThingShadowRequest>(device);
            _ = await this.externalDevicesService.UpdateDeviceShadow(shadowRequest);

            //Create Thing in DB
            return await CreateDeviceInDatabase(device);
        }

        protected override async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<Device>(device);

            await this.deviceRepository.InsertAsync(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        public override Task DeleteDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override Task<DeviceDetails> UpdateDevice(DeviceDetails device)
        {
            throw new NotImplementedException();
        }

        public override Task<DeviceDetails> GetDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessTelemetryEvent(EventData eventMessage)
        {
            throw new NotImplementedException();
        }
    }
}
