// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Models.v10;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain;
    using Amazon.IoT.Model;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using Amazon.IotData.Model;
    using Microsoft.Extensions.Configuration;
    using AzureIoTHub.Portal.Shared.Constants;

    public class AWSDeviceService : IDeviceService<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IConfiguration config;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IAWSExternalDeviceService externalDevicesService;

        public AWSDeviceService(IMapper mapper,
            IConfiguration config,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IAWSExternalDeviceService externalDevicesService)
        {
            this.mapper = mapper;
            this.config = config;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.externalDevicesService = externalDevicesService;
        }

        public Task<bool> CheckIfDeviceExists(string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceDetails> CreateDevice(DeviceDetails device)
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

        private async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<Device>(device);

            //In AWS FK is on ThingType
            if (this.config["CloudProvider"]!.Equals(CloudProviders.AWS, StringComparison.Ordinal))
            {
                deviceEntity.ThingTypeId = deviceEntity.DeviceModelId;
                deviceEntity.DeviceModelId = null;
            }

            await this.deviceRepository.InsertAsync(deviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        public Task DeleteDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            throw new NotImplementedException();
        }

        public Task<EnrollmentCredentials> GetCredentials(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceDetails> GetDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResult<DeviceListItem>> GetDevices(string? searchText = null, bool? searchStatus = null, bool? searchState = null, int pageSize = 10, int pageNumber = 0, string[]? orderBy = null, Dictionary<string, string>? tags = null, string? modelId = null, List<string>? labels = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task ProcessTelemetryEvent(EventData eventMessage)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceDetails> UpdateDevice(DeviceDetails device)
        {
            throw new NotImplementedException();
        }
    }
}
