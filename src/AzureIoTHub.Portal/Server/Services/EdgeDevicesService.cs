// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class EdgeDevicesService : IEdgeDevicesService
    {
        /// <summary>
        /// The device registry manager.
        /// </summary>
        private readonly RegistryManager registryManager;

        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// The edge device mapper.
        /// </summary>
        private readonly IEdgeDeviceMapper edgeDeviceMapper;

        /// <summary>
        /// The device provisioning srevice manager.
        /// </summary>
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        private readonly IDeviceTagService deviceTagService;

        public EdgeDevicesService(RegistryManager registryManager, IDeviceService devicesService,
            IEdgeDeviceMapper edgeDeviceMapper,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            IDeviceTagService deviceTagService)
        {
            this.registryManager = registryManager;
            this.devicesService = devicesService;
            this.edgeDeviceMapper = edgeDeviceMapper;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
            this.deviceTagService = deviceTagService;
            this.deviceTagService = deviceTagService;
        }

        public PaginationResult<IoTEdgeListItem> GetEdgeDevicesPage(PaginationResult<Twin> edgeDevicesTwin,
            IUrlHelper urlHelper,
            string searchText = null,
            bool? searchStatus = null,
            string searchType = null,
            int pageSize = 10)
        {
            var newEdgeDevicesList = new List<IoTEdgeListItem>();

            foreach (var deviceTwin in edgeDevicesTwin.Items)
            {
                newEdgeDevicesList.Add(this.edgeDeviceMapper.CreateEdgeDeviceListItem(deviceTwin));
            }

            var nextPage = string.Empty;

            if (!string.IsNullOrEmpty(edgeDevicesTwin.NextPage))
            {
                nextPage = urlHelper.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GET IoT Edge devices",
                    Values = new
                    {
                        continuationToken = edgeDevicesTwin.NextPage,
                        searchText,
                        searchType,
                        searchStatus,
                        pageSize
                    }
                });
            }

            return new PaginationResult<IoTEdgeListItem>
            {
                Items = newEdgeDevicesList,
                TotalItems = edgeDevicesTwin.TotalItems,
                NextPage = nextPage
            };
        }

        /// <summary>
        /// Get edge device with its modules.
        /// </summary>
        /// <param name="edgeDeviceId">device id.</param>
        /// <returns>IoTEdgeDevice object.</returns>
        public async Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId)
        {
            var deviceTwin = await this.devicesService.GetDeviceTwin(edgeDeviceId);

            if (deviceTwin is null)
            {
                throw new ResourceNotFoundException($"Device {edgeDeviceId} not found.");
            }

            var deviceTwinWithModules = await this.devicesService.GetDeviceTwinWithModule(edgeDeviceId);

            var edgeDeviceLastConfiguration = await RetrieveLastConfiguration(deviceTwinWithModules);
            var edgeDeviceNbConnectedDevice = await RetrieveNbConnectedDevice(edgeDeviceId);
            var tagList = this.deviceTagService.GetAllTagsNames();

            return this.edgeDeviceMapper.CreateEdgeDevice(deviceTwin, deviceTwinWithModules, edgeDeviceNbConnectedDevice, edgeDeviceLastConfiguration, tagList);
        }

        /// <summary>
        /// Create a new edge device.
        /// </summary>
        /// <param name="edgeDevice"> the new edge device.</param>
        /// <returns>the result of the operation.</returns>
        public async Task<BulkRegistryOperationResult> CreateEdgeDevice(IoTEdgeDevice edgeDevice)
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

            return await this.devicesService.CreateDeviceWithTwin(edgeDevice.DeviceId, true, deviceTwin, DeviceStatus.Enabled);
        }

        /// <summary>
        /// Update the edge device ant the twin.
        /// </summary>
        /// <param name="edgeDevice">edge device object update.</param>
        /// <returns>device twin updated.</returns>
        public async Task<Twin> UpdateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            var device = await this.devicesService.GetDevice(edgeDevice.DeviceId);

            if (Enum.TryParse(edgeDevice.Status, out DeviceStatus status))
            {
                device.Status = status;
            }

            _ = await this.devicesService.UpdateDevice(device);

            var deviceTwin = await this.devicesService.GetDeviceTwin(edgeDevice.DeviceId);

            return await this.devicesService.UpdateDeviceTwin(deviceTwin);
        }

        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="edgeModule"></param>
        /// <param name="deviceId"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<C2Dresult> ExecuteModuleMethod(IoTEdgeModule edgeModule, string deviceId, string methodName)
        {
            ArgumentNullException.ThrowIfNull(edgeModule, nameof(edgeModule));

            var method = new CloudToDeviceMethod(methodName);
            var payload = string.Empty;

            if (methodName == "RestartModule")
            {
                payload = JsonConvert.SerializeObject(new
                {
                    id = edgeModule.ModuleName,
                    schemaVersion = edgeModule.Version
                });
            }

            _ = method.SetPayloadJson(payload);

            var result = await this.devicesService.ExecuteC2DMethod(deviceId, method);

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
        }

        /// <summary>
        /// Gets the IoT Edge device enrollement credentials.
        /// </summary>
        /// <param name="edgeDeviceId">the edge device id.</param>
        /// <returns>Enrollment credentials.</returns>
        /// <exception cref="ResourceNotFoundException"></exception>
        public async Task<EnrollmentCredentials> GetEdgeDeviceCredentials(string edgeDeviceId)
        {
            var deviceTwin = await this.devicesService.GetDeviceTwin(edgeDeviceId);

            if (deviceTwin == null)
            {
                throw new ResourceNotFoundException($"IoT Edge {edgeDeviceId} doesn't exist.");
            }

            var modelId = DeviceHelper.RetrieveTagValue(deviceTwin, nameof(IoTEdgeDevice.ModelId));

            return await this.deviceProvisioningServiceManager.GetEnrollmentCredentialsAsync(edgeDeviceId, modelId);
        }

        /// <summary>
        /// Retrieves the connected devices number on the IoT Edge.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private async Task<int> RetrieveNbConnectedDevice(string deviceId)
        {
            var query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']", 1);
            var deviceWithClient = (await query.GetNextAsTwinAsync()).SingleOrDefault();
            var reportedProperties = JObject.Parse(deviceWithClient.Properties.Reported.ToJson());

            return reportedProperties.TryGetValue("clients", out var clients) ? clients.Count() : 0;
        }

        /// <summary>
        /// Retrieves the last configuration of the IoT Edge.
        /// </summary>
        /// <param name="twin">The twin.</param>
        private async Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            var item = new ConfigItem();

            if (twin.Configurations?.Count > 0)
            {
                foreach (var config in twin.Configurations)
                {
                    var confObj = await this.registryManager.GetConfigurationAsync(config.Key);
                    if (item.DateCreation < confObj.CreatedTimeUtc && config.Value.Status == ConfigurationStatus.Applied)
                    {
                        item.Name = config.Key;
                        item.DateCreation = confObj.CreatedTimeUtc;
                        item.Status = nameof(ConfigurationStatus.Applied);
                    }
                }
                return item;
            }
            return null;
        }
    }
}
