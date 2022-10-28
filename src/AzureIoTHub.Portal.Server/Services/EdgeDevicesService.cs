// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v1._0.Filters;
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

        private readonly PortalDbContext portalDbContext;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public EdgeDevicesService(
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            PortalDbContext portalDbContext,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IDeviceModelImageManager deviceModelImageManager)
        {
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.portalDbContext = portalDbContext;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public async Task<PaginatedResult<IoTEdgeListItem>> GetEdgeDevicesPage(
            string searchText = null,
            bool? searchStatus = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null)
        {
            var deviceListFilter = new EdgeDeviceListFilter
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                IsEnabled = searchStatus,
                Keyword = searchText,
                OrderBy = orderBy,
            };

            var devicePredicate = PredicateBuilder.True<EdgeDevice>();

            if (deviceListFilter.IsEnabled != null)
            {
                devicePredicate = devicePredicate.And(device => device.IsEnabled.Equals(deviceListFilter.IsEnabled));
            }

            if (!string.IsNullOrWhiteSpace(deviceListFilter.Keyword))
            {
                devicePredicate = devicePredicate.And(device => device.Id.ToLower().Contains(deviceListFilter.Keyword.ToLower()) || device.Name.ToLower().Contains(deviceListFilter.Keyword.ToLower()));
            }

            var paginatedEdgeDevices = await this.edgeDeviceRepository.GetPaginatedListAsync(pageNumber, pageSize, orderBy, devicePredicate);
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
            var deviceEntity = await this.edgeDeviceRepository.GetByIdAsync(edgeDeviceId);

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
            try
            {
                var edgeDeviceEntity = this.mapper.Map<EdgeDevice>(device);

                await this.edgeDeviceRepository.InsertAsync(edgeDeviceEntity);
                await this.unitOfWork.SaveAsync();

                return device;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create the device {device.DeviceName}", e);
            }
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
            try
            {
                var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(device.DeviceId);

                if (edgeDeviceEntity == null)
                {
                    throw new ResourceNotFoundException($"The device {device.DeviceId} doesn't exist");
                }

                foreach (var deviceTagEntity in edgeDeviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                _ = this.mapper.Map(device, edgeDeviceEntity);

                this.edgeDeviceRepository.Update(edgeDeviceEntity);
                await this.unitOfWork.SaveAsync();

                return device;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create the device {device.DeviceName}", e);
            }
        }

        public async Task DeleteEdgeDeviceAsync(string deviceId)
        {
            await this.externalDevicesService.DeleteDevice(deviceId);

            await DeleteEdgeDeviceInDatabase(deviceId);
        }

        public async Task DeleteEdgeDeviceInDatabase(string deviceId)
        {
            try
            {
                var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(deviceId);

                if (edgeDeviceEntity == null)
                {
                    return;
                }

                foreach (var deviceTagEntity in edgeDeviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                this.edgeDeviceRepository.Delete(deviceId);

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device {deviceId}", e);
            }
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
