// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models;
    using Domain.Entities;
    using Infrastructure;
    using Infrastructure.Repositories;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;
    using Device = Domain.Entities.Device;

    public abstract class DeviceServiceBase<TDto> : IDeviceService<TDto>
        where TDto : IDeviceDetails
    {
        private readonly PortalDbContext portalDbContext;
        private readonly IMapper mapper;
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceTwinMapper<DeviceListItem, TDto> deviceTwinMapper;
        private readonly ILogger<DeviceServiceBase<TDto>> logger;

        protected DeviceServiceBase(PortalDbContext portalDbContext,
            IMapper mapper,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTwinMapper<DeviceListItem, TDto> deviceTwinMapper,
            ILogger<DeviceServiceBase<TDto>> logger)
        {
            this.portalDbContext = portalDbContext;
            this.mapper = mapper;
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceTwinMapper = deviceTwinMapper;
            this.logger = logger;
        }

        public async Task<PaginatedResult<DeviceListItem>> GetDevices(string searchText = null, bool? searchStatus = null, bool? searchState = null, int pageSize = 10,
            int pageNumber = 0, string[] orderBy = null, Dictionary<string, string> tags = default, string modelId = null, List<string> labels = default)
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
                ModelId = modelId,
                Labels = labels
            };

            var devicePredicate = PredicateBuilder.True<Device>();

            if (deviceListFilter.IsConnected != null)
            {
                devicePredicate = devicePredicate.And(device => device.IsConnected.Equals(deviceListFilter.IsConnected));
            }

            if (deviceListFilter.IsEnabled != null)
            {
                devicePredicate = devicePredicate.And(device => device.IsEnabled.Equals(deviceListFilter.IsEnabled));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.Keyword))
            {
                devicePredicate = devicePredicate.And(device => device.Id.ToLower().Contains(deviceListFilter.Keyword.ToLower()) || device.Name.ToLower().Contains(deviceListFilter.Keyword.ToLower()));
            }

            foreach (var keyValuePair in deviceListFilter.Tags)
            {
                devicePredicate = devicePredicate.And(device => device.Tags.Any(value =>
                    value.Name.Equals(keyValuePair.Key) && value.Value.Equals(keyValuePair.Value)));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.ModelId))
            {
                devicePredicate = devicePredicate.And(device => device.DeviceModelId.Equals(deviceListFilter.ModelId));
            }

            deviceListFilter.Labels?.ForEach(label =>
            {
                devicePredicate = devicePredicate.And(device => device.Labels.Any(value => value.Name.Equals(label)) || device.DeviceModel.Labels.Any(value => value.Name.Equals(label)));
            });

            var ordering = deviceListFilter.OrderBy?.Any() == true ? string.Join(",", deviceListFilter.OrderBy) : null;

            var query = this.portalDbContext.Devices
                .Where(devicePredicate)
                .OrderBy(!string.IsNullOrWhiteSpace(ordering) ? ordering : nameof(IDevice.Id))
                .Include(device => device.Labels);

            var resultCount = await query
                                    .AsNoTracking()
                                    .CountAsync();

            var devices = await query
                .Skip(deviceListFilter.PageNumber * deviceListFilter.PageSize)
                .Take(deviceListFilter.PageSize)
                .Select(device => new DeviceListItem
                {
                    DeviceID = device.Id,
                    DeviceName = device.Name,
                    IsEnabled = device.IsEnabled,
                    IsConnected = device.IsConnected,
                    StatusUpdatedTime = device.StatusUpdatedTime,
                    DeviceModelId = device.DeviceModelId,
                    SupportLoRaFeatures = device is LorawanDevice,
                    HasLoRaTelemetry = device is LorawanDevice && ((LorawanDevice) device).Telemetry.Any(),
                    Labels = device.Labels
                        .Union(device.DeviceModel.Labels)
                        .Select(x => new LabelDto
                        {
                            Color = x.Color,
                            Name = x.Name,
                        })
                })
                .ToListAsync();

            devices.ForEach(device =>
            {
                device.ImageUrl = this.deviceModelImageManager.ComputeImageUri(device.DeviceModelId);
            });

            return new PaginatedResult<DeviceListItem>(devices, resultCount);
        }

        public abstract Task<TDto> GetDevice(string deviceId);

        public abstract Task<bool> CheckIfDeviceExists(string deviceId);

        public virtual async Task<TDto> CreateDevice(TDto device)
        {
            var newTwin = await this.externalDevicesService.CreateNewTwinFromDeviceId(device.DeviceID);

            this.deviceTwinMapper.UpdateTwin(newTwin, device);
            var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

            return await CreateDeviceInDatabase(device);
        }

        protected abstract Task<TDto> CreateDeviceInDatabase(TDto device);

        public virtual async Task<TDto> UpdateDevice(TDto device)
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
            try
            {
                await this.externalDevicesService.DeleteDevice(deviceId);
            }
            catch (DeviceNotFoundException e)
            {
                this.logger.LogWarning($"Unable to delete the device with ID {deviceId} because it doesn't exist on IoT Hub {e.Message}");
            }

            await DeleteDeviceInDatabase(deviceId);
        }

        protected abstract Task DeleteDeviceInDatabase(string deviceId);

        public virtual Task<DeviceCredentials> GetCredentials(TDto device)
        {
            return this.externalDevicesService.GetDeviceCredentials(device);
        }

        public abstract Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId);

        public abstract Task ProcessTelemetryEvent(EventData eventMessage);

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            var deviceLabelsQuery = this.portalDbContext.Devices
                .Include(device => device.Labels)
                .Include(device => device.DeviceModel.Labels)
                .Select(device => new
                {
                    DeviceLabels = device.Labels,
                    ModelLabels = device.DeviceModel.Labels
                });

            var loRaDeviceLabelsQuery = this.portalDbContext.LorawanDevices
                .Include(device => device.Labels)
                .Include(device => device.DeviceModel.Labels)
                .Select(device => new
                {
                    DeviceLabels = device.Labels,
                    ModelLabels = device.DeviceModel.Labels
                });

            var deviceLabels = await deviceLabelsQuery.ToListAsync();
            var loRaDeviceLabels = await loRaDeviceLabelsQuery.ToListAsync();

            var labels = deviceLabels.SelectMany(x => x.DeviceLabels)
                .Union(deviceLabels.SelectMany(x => x.ModelLabels))
                .Union(loRaDeviceLabels.SelectMany(x => x.DeviceLabels))
                .Union(loRaDeviceLabels.SelectMany(x => x.ModelLabels));


            return this.mapper.Map<List<LabelDto>>(labels);
        }

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
