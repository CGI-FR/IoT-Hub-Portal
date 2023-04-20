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

    public class AWSDeviceService : IDeviceService<DeviceDetails>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IExternalDeviceService externalDevicesService;

        public AWSDeviceService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IExternalDeviceService externalDevicesService)
        {
            this.mapper = mapper;
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
            var newTwin = await this.externalDevicesService.CreateNewTwinFromDeviceId(device.DeviceID);

            _ = await this.externalDevicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, string.Empty);

            return await CreateDeviceInDatabase(device);
        }

        private async Task<DeviceDetails> CreateDeviceInDatabase(DeviceDetails device)
        {
            var deviceEntity = this.mapper.Map<Device>(device);

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
