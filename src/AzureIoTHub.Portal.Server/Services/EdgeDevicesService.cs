// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Helpers;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class EdgeDevicesService : IEdgeDevicesService
    {
        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IExternalDeviceService externalDevicesService;

        private readonly IDeviceTagService deviceTagService;

        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public EdgeDevicesService(
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceModelImageManager deviceModelImageManager)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.labelRepository = labelRepository;
            this.deviceModelImageManager = deviceModelImageManager;
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
                devicePredicate = devicePredicate.And(device => device.IsEnabled.Equals(deviceListFilter.IsEnabled));
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
        public async Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId)
        {
            var deviceEntity = await this.edgeDeviceRepository.GetByIdAsync(edgeDeviceId, d => d.Tags, d => d.Labels);

            if (deviceEntity is null)
            {
                throw new ResourceNotFoundException($"The device with id {edgeDeviceId} doesn't exist");
            }

            var deviceDto = this.mapper.Map<IoTEdgeDevice>(deviceEntity);

            var deviceTwinWithModules = await this.externalDevicesService.GetDeviceTwinWithModule(edgeDeviceId);

            deviceDto.LastDeployment = await this.externalDevicesService.RetrieveLastConfiguration(deviceTwinWithModules);
            deviceDto.ImageUrl = this.deviceModelImageManager.ComputeImageUri(deviceDto.ModelId);
            deviceDto.Modules = DeviceHelper.RetrieveModuleList(deviceTwinWithModules);
            deviceDto.RuntimeResponse = DeviceHelper.RetrieveRuntimeResponse(deviceTwinWithModules);

            deviceDto.Status = deviceTwinWithModules.Status?.ToString();
            deviceDto.Tags = FilterDeviceTags(deviceDto);

            return deviceDto;
        }

        /// <summary>
        /// Create a new edge device.
        /// </summary>
        /// <param name="edgeDevice"> the new edge device.</param>
        /// <returns>the result of the operation.</returns>
        public async Task<IoTEdgeDevice> CreateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            var deviceTwin = new Twin(edgeDevice.DeviceId);

            if (edgeDevice.Tags != null)
            {
                foreach (var tag in edgeDevice.Tags)
                {
                    DeviceHelper.SetTagValue(deviceTwin, tag.Key, tag.Value);
                }
            }

            DeviceHelper.SetTagValue(deviceTwin, nameof(IoTEdgeDevice.ModelId), edgeDevice.ModelId);
            DeviceHelper.SetTagValue(deviceTwin, nameof(IoTEdgeDevice.DeviceName), edgeDevice.DeviceName);

            _ = await this.externalDevicesService.CreateDeviceWithTwin(edgeDevice.DeviceId, true, deviceTwin, DeviceStatus.Enabled);

            return await CreateEdgeDeviceInDatabase(edgeDevice);
        }

        private async Task<IoTEdgeDevice> CreateEdgeDeviceInDatabase(IoTEdgeDevice device)
        {
            var edgeDeviceEntity = this.mapper.Map<EdgeDevice>(device);

            await this.edgeDeviceRepository.InsertAsync(edgeDeviceEntity);
            await this.unitOfWork.SaveAsync();

            return device;
        }

        /// <summary>
        /// Update the edge device ant the twin.
        /// </summary>
        /// <param name="edgeDevice">edge device object update.</param>
        /// <returns>device twin updated.</returns>
        public async Task<IoTEdgeDevice> UpdateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            var device = await this.externalDevicesService.GetDevice(edgeDevice.DeviceId);

            if (Enum.TryParse(edgeDevice.Status, out DeviceStatus status))
            {
                device.Status = status;
            }

            _ = await this.externalDevicesService.UpdateDevice(device);

            var deviceTwin = await this.externalDevicesService.GetDeviceTwin(edgeDevice.DeviceId);

            _ = await this.externalDevicesService.UpdateDeviceTwin(deviceTwin);

            return await UpdateEdgeDeviceInDatabase(edgeDevice);
        }

        private async Task<IoTEdgeDevice> UpdateEdgeDeviceInDatabase(IoTEdgeDevice device)
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
            await this.unitOfWork.SaveAsync();

            return device;
        }

        public async Task DeleteEdgeDeviceAsync(string deviceId)
        {
            await this.externalDevicesService.DeleteDevice(deviceId);

            await DeleteEdgeDeviceInDatabase(deviceId);
        }

        public async Task DeleteEdgeDeviceInDatabase(string deviceId)
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

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="deviceId"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        {
            ArgumentNullException.ThrowIfNull(moduleName, nameof(moduleName));

            if (!methodName.Equals("RestartModule", StringComparison.Ordinal))
            {
                return await ExecuteModuleCommand(deviceId, moduleName, methodName);
            }

            var method = new CloudToDeviceMethod(methodName);

            var payload = JsonConvert.SerializeObject(new
            {
                id = moduleName,
                schemaVersion = "1.0"
            });

            _ = method.SetPayloadJson(payload);

            var result = await this.externalDevicesService.ExecuteC2DMethod(deviceId, method);

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
        }

        /// <summary>
        /// Execute the custom module command.
        /// </summary>
        /// <param name="deviceId">the device identifier.</param>
        /// <param name="moduleName">the module name.</param>
        /// <param name="commandName">the command name.</param>
        /// <returns></returns>
        public async Task<C2Dresult> ExecuteModuleCommand(string deviceId, string moduleName, string commandName)
        {
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(moduleName, nameof(moduleName));
            ArgumentNullException.ThrowIfNull(commandName, nameof(commandName));

            var method = new CloudToDeviceMethod(commandName);
            var payload = "{}";

            _ = method.SetPayloadJson(payload);

            var result = await this.externalDevicesService.ExecuteCustomCommandC2DMethod(deviceId,moduleName, method);

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
        }

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            var edgeDevices = await this.edgeDeviceRepository.GetAllAsync(includes: new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels });

            var labels = edgeDevices.SelectMany(edgeDevice => edgeDevice.Labels)
                .Union(edgeDevices.SelectMany(edgeDevice => edgeDevice.DeviceModel.Labels));

            return this.mapper.Map<List<LabelDto>>(labels);
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
