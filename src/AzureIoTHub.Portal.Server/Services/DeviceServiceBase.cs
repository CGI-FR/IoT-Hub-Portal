// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models;
    using Domain.Entities;
    using Infrastructure;
    using Infrastructure.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Models.v10;
    using Shared.Models.v1._0;
    using Managers;
    using System.Linq.Dynamic.Core;
    using Microsoft.Azure.Devices;
    using Mappers;
    using Shared.Models.v1._0.Filters;
    using Device = Domain.Entities.Device;

    public abstract class DeviceServiceBase<TDto> : IDeviceService<TDto>
        where TDto : IDeviceDetails
    {
        private readonly PortalDbContext portalDbContext;
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceTwinMapper<DeviceListItem, TDto> deviceTwinMapper;

        protected DeviceServiceBase(PortalDbContext portalDbContext,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, TDto> deviceTwinMapper)
        {
            this.portalDbContext = portalDbContext;
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceTwinMapper = deviceTwinMapper;
        }

        public async Task<PaginatedResult<DeviceListItem>> GetDevices(string searchText = null, bool? searchStatus = null, bool? searchState = null, int pageSize = 10,
            int pageNumber = 0, string[] orderBy = null, Dictionary<string, string> tags = default, string modelId = null)
        {
            var deviceListFilter = new DeviceListFilter
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                IsConnected = searchState,
                IsEnabled = searchStatus,
                Keyword = searchText,
                OrderBy = orderBy,
                Tags = GetSearchableTags(tags),
                ModelId = modelId
            };

            var devicePredicate = PredicateBuilder.True<Device>();
            var lorawanDevicePredicate = PredicateBuilder.True<LorawanDevice>();

            if (deviceListFilter.IsConnected != null)
            {
                devicePredicate = devicePredicate.And(device => device.IsConnected.Equals(deviceListFilter.IsConnected));
                lorawanDevicePredicate = lorawanDevicePredicate.And(device => device.IsConnected.Equals(deviceListFilter.IsConnected));
            }

            if (deviceListFilter.IsEnabled != null)
            {
                devicePredicate = devicePredicate.And(device => device.IsEnabled.Equals(deviceListFilter.IsEnabled));
                lorawanDevicePredicate = lorawanDevicePredicate.And(device => device.IsEnabled.Equals(deviceListFilter.IsEnabled));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.Keyword))
            {
                devicePredicate = devicePredicate.And(device => device.Id.ToLower().Contains(deviceListFilter.Keyword.ToLower()) || device.Name.ToLower().Contains(deviceListFilter.Keyword.ToLower()));
                lorawanDevicePredicate = lorawanDevicePredicate.And(device => device.Id.ToLower().Contains(deviceListFilter.Keyword.ToLower()) || device.Name.ToLower().Contains(deviceListFilter.Keyword.ToLower()));
            }

            foreach (var keyValuePair in deviceListFilter.Tags)
            {
                devicePredicate = devicePredicate.And(device => device.Tags.Any(value =>
                    value.Name.Equals(keyValuePair.Key) && value.Value.Equals(keyValuePair.Value)));

                lorawanDevicePredicate = lorawanDevicePredicate.And(device => device.Tags.Any(value =>
                    value.Name.Equals(keyValuePair.Key) && value.Value.Equals(keyValuePair.Value)));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.ModelId))
            {
                devicePredicate = devicePredicate.And(device => device.DeviceModelId.Equals(deviceListFilter.ModelId));
                lorawanDevicePredicate = lorawanDevicePredicate.And(device => device.DeviceModelId.Equals(deviceListFilter.ModelId));
            }

            var query = this.portalDbContext.Devices
                .Include(device => device.Tags)
                .Where(devicePredicate)
                .Select(device => new DeviceListItem
                {
                    DeviceID = device.Id,
                    DeviceName = device.Name,
                    IsEnabled = device.IsEnabled,
                    IsConnected = device.IsConnected,
                    StatusUpdatedTime = device.StatusUpdatedTime,
                    DeviceModelId = device.DeviceModelId,
                    SupportLoRaFeatures = false,
                })
                .Union(this.portalDbContext.LorawanDevices
                    .Include(device => device.Tags)
                    .Where(lorawanDevicePredicate)
                    .Select(device => new DeviceListItem
                    {
                        DeviceID = device.Id,
                        DeviceName = device.Name,
                        IsEnabled = device.IsEnabled,
                        IsConnected = device.IsConnected,
                        StatusUpdatedTime = device.StatusUpdatedTime,
                        DeviceModelId = device.DeviceModelId,
                        SupportLoRaFeatures = true
                    }));

            var ordering = deviceListFilter.OrderBy?.Any() == true ? string.Join(",", deviceListFilter.OrderBy) : null;

            query = !string.IsNullOrWhiteSpace(ordering) ? query.OrderBy(ordering) : query.OrderBy(device => device.DeviceID);

            var resultCount = await query.AsNoTracking().CountAsync();

            var devices = await query.Skip(deviceListFilter.PageNumber * deviceListFilter.PageSize).Take(deviceListFilter.PageSize).ToListAsync();

            devices.ForEach(device =>
            {
                device.ImageUrl = this.deviceModelImageManager.ComputeImageUri(device.DeviceModelId);
            });

            return new PaginatedResult<DeviceListItem>(devices, resultCount);
        }

        public abstract Task<TDto> GetDevice(string deviceId);

        public async Task<TDto> CreateDevice(TDto device)
        {
            var newTwin = await this.externalDevicesService.CreateNewTwinFromDeviceId(device.DeviceID);

            this.deviceTwinMapper.UpdateTwin(newTwin, device);
            var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

            return await CreateDeviceInDatabase(device);
        }

        protected abstract Task<TDto> CreateDeviceInDatabase(TDto device);

        public async Task<TDto> UpdateDevice(TDto device)
        {
            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentDevice = await this.externalDevicesService.GetDevice(device.DeviceID);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.externalDevicesService.GetDeviceTwin(device.DeviceID);

            // Update the twin properties
            this.deviceTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.externalDevicesService.UpdateDeviceTwin(currentTwin);

            return await UpdateDeviceInDatabase(device);
        }

        protected abstract Task<TDto> UpdateDeviceInDatabase(TDto device);

        public virtual async Task DeleteDevice(string deviceId)
        {
            await this.externalDevicesService.DeleteDevice(deviceId);

            await DeleteDeviceInDatabase(deviceId);
        }

        protected abstract Task DeleteDeviceInDatabase(string deviceId);

        public virtual Task<EnrollmentCredentials> GetCredentials(string deviceId)
        {
            return this.externalDevicesService.GetEnrollmentCredentials(deviceId);
        }

        public abstract Task<IEnumerable<DeviceTelemetryDto>> GetDeviceTelemetries(string deviceId);

        protected Dictionary<string, string> FilterDeviceTags(TDto device)
        {
            var tags = this.deviceTagService.GetAllTagsNames().ToList();

            return device.Tags.Where(pair => tags.Contains(pair.Key))
                .Union(tags.Where(s => !device.Tags.ContainsKey(s))
                    .Select(s => new KeyValuePair<string, string>(s, string.Empty)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private Dictionary<string, string> GetSearchableTags(IReadOnlyDictionary<string, string> tags)
        {
            return tags == null
                ? new Dictionary<string, string>()
                : this.deviceTagService.GetAllSearchableTagsNames()
                    .Where(tags.ContainsKey)
                    .Select(tag => new { tag, tagValue = tags[tag] })
                    .ToDictionary(pair => pair.tag, pair => pair.tagValue);
        }
    }
}
