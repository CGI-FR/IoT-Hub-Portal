// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;

    public abstract class EdgeDevicesServiceBase
    {
        private readonly IDeviceTagService deviceTagService;

        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IMapper mapper;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly ILabelRepository labelRepository;

        protected EdgeDevicesServiceBase(
            IDeviceTagService deviceTagService,
            IEdgeDeviceRepository edgeDeviceRepository,
            IMapper mapper,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository)
        {
            this.deviceTagService = deviceTagService;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.mapper = mapper;
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.labelRepository = labelRepository;
        }

        public async Task<PaginatedResult<IoTEdgeListItem>> GetEdgeDevicesPage(
                string searchText = null,
                bool? searchStatus = null,
                int pageSize = 10,
                int pageNumber = 0,
                string[] orderBy = null,
                string modelId = null,
                List<string> labels = default)
        {
            var deviceListFilter = new EdgeDeviceListFilter
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                IsEnabled = searchStatus,
                Keyword = searchText,
                OrderBy = orderBy,
                ModelId = modelId,
                Labels = labels
            };

            var devicePredicate = PredicateBuilder.True<EdgeDevice>();

            if (deviceListFilter.IsEnabled != null)
            {
                devicePredicate = devicePredicate.And(device => device.ConnectionState.Equals(deviceListFilter.IsEnabled.Value ? "Connected" : "Disconnected"));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.ModelId))
            {
                devicePredicate = devicePredicate.And(device => device.DeviceModelId.Equals(deviceListFilter.ModelId));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.Keyword))
            {
                devicePredicate = devicePredicate.And(device => device.Id.ToLower().Contains(deviceListFilter.Keyword.ToLower()) || device.Name.ToLower().Contains(deviceListFilter.Keyword.ToLower()));
            }

            deviceListFilter.Labels?.ForEach(label =>
            {
                devicePredicate = devicePredicate.And(device => device.Labels.Any(value => value.Name.Equals(label)) || device.DeviceModel.Labels.Any(value => value.Name.Equals(label)));
            });

            var paginatedEdgeDevices = await this.edgeDeviceRepository.GetPaginatedListAsync(pageNumber, pageSize, orderBy, devicePredicate, includes: new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels });
            var paginateEdgeDeviceDto = new PaginatedResult<IoTEdgeListItem>
            {
                Data = paginatedEdgeDevices.Data.Select(x => this.mapper.Map<IoTEdgeListItem>(x, opts =>
                {
                    opts.Items["imageUrl"] = this.deviceModelImageManager.ComputeImageUri(x.DeviceModelId);
                })).ToList(),
                TotalCount = paginatedEdgeDevices.TotalCount,
                CurrentPage = paginatedEdgeDevices.CurrentPage,
                PageSize = pageSize
            };

            return new PaginatedResult<IoTEdgeListItem>(paginateEdgeDeviceDto.Data, paginateEdgeDeviceDto.TotalCount);
        }

        /// <summary>
        /// Get edge device with its modules.
        /// </summary>
        /// <param name="edgeDeviceId">device id.</param>
        /// <returns>IoTEdgeDevice object.</returns>
        protected async Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId)
        {
            var deviceEntity = await this.edgeDeviceRepository.GetByIdAsync(edgeDeviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity is null)
            {
                throw new ResourceNotFoundException($"The device with id {edgeDeviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<IoTEdgeDevice>(deviceEntity);
            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);
            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;

        }

        protected async Task<IoTEdgeDevice> CreateEdgeDeviceInDatabase(IoTEdgeDevice device)
        {
            var edgeDeviceEntity = this.mapper.Map<EdgeDevice>(device);

            await this.edgeDeviceRepository.InsertAsync(edgeDeviceEntity);

            return device;
        }

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            var edgeDevices = await this.edgeDeviceRepository.GetAllAsync(includes: new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels });

            var labels = edgeDevices.SelectMany(edgeDevice => edgeDevice.Labels)
                .Union(edgeDevices.SelectMany(edgeDevice => edgeDevice.DeviceModel.Labels));

            return this.mapper.Map<List<LabelDto>>(labels);
        }

        protected async Task<IoTEdgeDevice> UpdateEdgeDeviceInDatabase(IoTEdgeDevice device)
        {
            var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(device.DeviceId, d => d.Tags, d => d.Labels);

            if (edgeDeviceEntity == null)
            {
                throw new ResourceNotFoundException($"The device {device.DeviceId} doesn't exist");
            }

            foreach (var deviceTagEntity in edgeDeviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in edgeDeviceEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            _ = this.mapper.Map(device, edgeDeviceEntity);

            this.edgeDeviceRepository.Update(edgeDeviceEntity);

            return device;
        }

        protected async Task DeleteEdgeDeviceInDatabase(string deviceId)
        {
            var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels);

            if (edgeDeviceEntity == null)
            {
                return;
            }

            foreach (var deviceTagEntity in edgeDeviceEntity.Tags)
            {
                this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
            }

            foreach (var labelEntity in edgeDeviceEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            this.edgeDeviceRepository.Delete(deviceId);
        }

        private Dictionary<string, string> FilterDeviceTags(IoTEdgeDevice device)
        {
            var tags = this.deviceTagService.GetAllTagsNames().ToList();

            return device.Tags.Where(pair => tags.Contains(pair.Key))
                .Union(tags.Where(s => !device.Tags.ContainsKey(s))
                    .Select(s => new KeyValuePair<string, string>(s, string.Empty)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
