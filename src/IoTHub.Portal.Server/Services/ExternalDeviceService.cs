// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Helpers;
    using Application.Providers;
    using Application.Services;
    using Azure;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Domain.Shared;
    using IoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shared;
    using Shared.Constants;
    using Shared.Models.v1._0;

    public class ExternalDeviceService : IExternalDeviceService
    {
        private readonly RegistryManager registryManager;
        private readonly ServiceClient serviceClient;
        private readonly ILogger<ExternalDeviceService> log;
        private readonly IDeviceRegistryProvider deviceRegistryProvider;
        private readonly IDeviceModelRepository deviceModelRepository;

        public ExternalDeviceService(
            ILogger<ExternalDeviceService> log,
            RegistryManager registryManager,
            ServiceClient serviceClient,
            IDeviceModelRepository deviceModelRepository,
            IDeviceRegistryProvider deviceRegistryProvider)
        {
            this.log = log;
            this.serviceClient = serviceClient;
            this.deviceModelRepository = deviceModelRepository;
            this.deviceRegistryProvider = deviceRegistryProvider;
            this.registryManager = registryManager;
        }

        /// <summary>
        /// this function return a list of all edge device without modules.
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchType"></param>
        /// <param name="pageSize"></param>
        /// <returns>IEnumerable twin.</returns>
        public async Task<PaginationResult<Twin>> GetAllEdgeDevice(
            string continuationToken = null,
            string searchText = null,
            bool? searchStatus = null,
            string searchType = null,
            int pageSize = 10)
        {
            var filter = "WHERE devices.capabilities.iotEdge = true";

            if (searchStatus != null)
            {
                filter += $" AND status = '{(searchStatus.Value ? "enabled" : "disabled")}'";
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
#pragma warning disable CA1308 // Normalize strings to uppercase
                filter += $" AND (STARTSWITH(deviceId, '{searchText}') OR (is_defined(tags.deviceName) AND STARTSWITH(tags.deviceName, '{searchText}')))";
#pragma warning restore CA1308 // Normalize strings to uppercase
            }

            if (!string.IsNullOrWhiteSpace(searchType))
            {
                filter += $" AND devices.tags.type = '{searchType}'";
            }

            var emptyResult = new PaginationResult<Twin>
            {
                Items = Enumerable.Empty<Twin>(),
                TotalItems = 0
            };

            IEnumerable<string> count;

            try
            {
                count = await this.registryManager
                    .CreateQuery($"SELECT COUNT() as totalNumber FROM devices {filter}")
                    .GetNextAsJsonAsync();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get the count of edge devices", e);
            }

            if (!JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result))
            {
                return emptyResult;
            }

            if (result.Value<int>() == 0)
            {
                return emptyResult;
            }
            try
            {
                var response = await this.registryManager
                .CreateQuery($"SELECT * FROM devices { filter }", pageSize)
                .GetNextAsTwinAsync(new QueryOptions
                {
                    ContinuationToken = continuationToken
                });


                return new PaginationResult<Twin>

                {
                    Items = response,
                    TotalItems = result.Value<int>(),
                    NextPage = response.ContinuationToken
                };
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get edge devices", e);
            }
        }

        /// <summary>
        /// this function return a list of all device exept edge device.
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="filterDeviceType"></param>
        /// <param name="excludeDeviceType"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="searchTags"></param>
        /// <param name="pageSize"></param>
        /// <returns>IEnumerable twin.</returns>
        public async Task<PaginationResult<Twin>> GetAllDevice(
            string continuationToken = null,
            string filterDeviceType = null,
            string excludeDeviceType = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            Dictionary<string, string> searchTags = null,
            int pageSize = 10)
        {
            var filter = "WHERE devices.capabilities.iotEdge = false";

            if (!string.IsNullOrWhiteSpace(filterDeviceType))
            {
                filter += $" AND devices.tags.deviceType = '{filterDeviceType}'";
            }

            if (!string.IsNullOrWhiteSpace(excludeDeviceType))
            {
                filter += $" AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != '{excludeDeviceType}')";
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
#pragma warning disable CA1308 // Normalize strings to uppercase
                filter += $" AND (STARTSWITH(deviceId, '{searchText}') OR (is_defined(tags.deviceName) AND STARTSWITH(tags.deviceName, '{searchText}')))";
#pragma warning restore CA1308 // Normalize strings to uppercase
            }

            if (searchTags != null)
            {
                var tagsFilterBuilder = new StringBuilder();

                foreach (var item in searchTags)
                {
                    _ = tagsFilterBuilder.Append(CultureInfo.InvariantCulture, $" AND is_defined(tags.{item.Key}) AND STARTSWITH(tags.{item.Key}, '{item.Value}')");
                }

                filter += tagsFilterBuilder;
            }

            if (searchStatus != null)
            {
                filter += $" AND status = '{(searchStatus.Value ? "enabled" : "disabled")}'";
            }

            if (searchState != null)
            {
                filter += $" AND connectionState = '{(searchState.Value ? "Connected" : "Disconnected")}'";
            }

            var emptyResult = new PaginationResult<Twin>
            {
                Items = Enumerable.Empty<Twin>(),
                TotalItems = 0
            };

            var stopWatch = Stopwatch.StartNew();

            IEnumerable<string> count;

            try
            {
                count = await this.registryManager
                    .CreateQuery($"SELECT COUNT() as totalNumber FROM devices {filter}")
                    .GetNextAsJsonAsync();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get devices count", e);
            }

            this.log.LogDebug($"Count obtained in {stopWatch.Elapsed}");

            if (!JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result))
            {
                return emptyResult;
            }

            if (result.Value<int>() == 0)
            {
                return emptyResult;
            }

            stopWatch.Restart();

            try
            {
                var query = this.registryManager
                    .CreateQuery($"SELECT * FROM devices { filter }", pageSize);

                var response = await query
                    .GetNextAsTwinAsync(new QueryOptions
                    {
                        ContinuationToken = continuationToken
                    });

                this.log.LogDebug($"Data obtained in {stopWatch.Elapsed}");

                return new PaginationResult<Twin>
                {
                    Items = response,
                    TotalItems = result.Value<int>(),
                    NextPage = response.ContinuationToken
                };
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to query devices", e);
            }
        }

        /// <summary>
        /// this function return the device we want with the registry manager.
        /// </summary>
        /// <param name="deviceId">device id.</param>
        /// <returns>Device.</returns>
        public async Task<Device> GetDevice(string deviceId)
        {
            try
            {
                return await this.registryManager.GetDeviceAsync(deviceId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get device with id {deviceId}", e);
            }
        }

        /// <summary>
        /// this function use the registry manager to find the twin
        /// of a device.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of a device.</returns>
        public async Task<Twin> GetDeviceTwin(string deviceId)
        {
            try
            {
                return await this.registryManager.GetTwinAsync(deviceId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get device twin with id {deviceId}", e);
            }
        }

        /// <summary>
        /// this function execute a query with the registry manager to find
        /// the twin of the device with this module.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of the device.</returns>
        public async Task<Twin> GetDeviceTwinWithModule(string deviceId)
        {
            var devicesWithModules = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");

            while (devicesWithModules.HasMoreResults)
            {
                try
                {
                    var devicesTwins = await devicesWithModules.GetNextAsTwinAsync();
                    return devicesTwins.ElementAt(0);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to get devices twins", e);
                }
            }

            return null;
        }

        public async Task<Twin> GetDeviceTwinWithEdgeHubModule(string deviceId)
        {
            var query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']", 1);

            try
            {
                var devicesTwins = await query.GetNextAsTwinAsync();
                return devicesTwins.ElementAt(0);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get devices twins", e);
            }
        }

        /// <summary>
        /// This function create a new device with his twin.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="isEdge">boolean.</param>
        /// <param name="twin">the twin of my new device.</param>
        /// <param name="isEnabled">the status of the device(disabled by default).</param>
        /// <returns>BulkRegistryOperation.</returns>
        public async Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled = DeviceStatus.Disabled)
        {
            var device = new Device(deviceId)
            {
                Capabilities = new DeviceCapabilities { IotEdge = isEdge },
                Status = isEnabled
            };

            try
            {
                return await this.registryManager.AddDeviceWithTwinAsync(device, twin);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to create the device twin with id {deviceId}: {e.Message}", e);
            }
        }

        /// <summary>
        /// This function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        public async Task DeleteDevice(string deviceId)
        {
            try
            {
                await this.registryManager.RemoveDeviceAsync(deviceId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device with id {deviceId}", e);
            }
        }

        /// <summary>
        /// This function update a device.
        /// </summary>
        /// <param name="device">the device id.</param>
        /// <returns>the updated device.</returns>
        public async Task<Device> UpdateDevice(Device device)
        {
            try
            {
                return await this.registryManager.UpdateDeviceAsync(device);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to update the device with id {device.Id}", e);
            }
        }

        /// <summary>
        /// This function update the twin of the device.
        /// </summary>
        /// <param name="twin">the new twin.</param>
        /// <returns>the updated twin.</returns>
        public async Task<Twin> UpdateDeviceTwin(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            try
            {
                return await this.registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to update the device twin with id {twin.DeviceId}", e);
            }
        }

        /// <summary>
        /// this function execute a methode on the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="method">the cloud to device method.</param>
        /// <returns>CloudToDeviceMethodResult.</returns>
        public async Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method)
        {
            try
            {
                return await this.serviceClient.InvokeDeviceMethodAsync(deviceId, "$edgeAgent", method);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to execute the cloud to device method {method.MethodName} on the device with id {deviceId}", e);
            }
        }

        /// <summary>
        /// C2DMethod for custom command.
        /// </summary>
        /// <param name="deviceId">the deviceId.</param>
        /// <param name="moduleName">the module name.</param>
        /// <param name="method">the C2DMethod.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task<CloudToDeviceMethodResult> ExecuteCustomCommandC2DMethod(string deviceId, string moduleName, CloudToDeviceMethod method)
        {
            try
            {
                return await this.serviceClient.InvokeDeviceMethodAsync(deviceId, moduleName, method);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to execute the cloud to device method {method.MethodName} on the device with id {deviceId}", e);
            }
        }

        /// <summary>
        /// Get edge device logs
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="edgeModule">Edge module</param>
        /// <returns>Edge device logs</returns>
        public async Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        {
            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var logs = new List<IoTEdgeDeviceLog>();

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = "1.0",
                items = new[]
                {
                    new
                    {
                        id = edgeModule.ModuleName,
                        filter = new
                        {
                            tail = 300
                        }
                    }
                },
                encoding = "none",
                contentType = "json"
            });

            _ = method.SetPayloadJson(payload);

            var result = await ExecuteC2DMethod(deviceId, method);

            if (result.Status == 200)
            {
                var payloadResponse = result.GetPayloadAsJson();

                if (string.IsNullOrWhiteSpace(payloadResponse))
                {
                    this.log.LogInformation($"Payload logs' response of the device {deviceId} is empty");
                }
                else
                {
                    var payloadResponseAsJson = JsonConvert.DeserializeObject<dynamic[]>(payloadResponse).Single().payload.ToString();

                    logs.AddRange(JsonConvert.DeserializeObject<List<IoTEdgeDeviceLog>>(payloadResponseAsJson));
                }
            }
            else
            {
                this.log.LogError($"Unable to retrieve logs of the device {deviceId}, status code: {result.Status}");
            }

            return logs.OrderByDescending(log => log.TimeStamp);
        }

        public async Task<int> GetDevicesCount()
        {
            try
            {
                var count = await this.registryManager
                    .CreateQuery("SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")
                    .GetNextAsJsonAsync();

                return !JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result) ? 0 : result.Value<int>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get devices count", e);
            }
        }

        public async Task<int> GetConnectedDevicesCount()
        {
            try
            {
                var count = await this.registryManager
                    .CreateQuery("SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND connectionState = 'Connected' AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")
                    .GetNextAsJsonAsync();

                return !JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result) ? 0 : result.Value<int>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get connected devices count", e);
            }
        }

        public async Task<int> GetEdgeDevicesCount()
        {
            try
            {
                var count = await this.registryManager
                    .CreateQuery("SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")
                    .GetNextAsJsonAsync();

                return !JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result) ? 0 : result.Value<int>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get edge devices count", e);
            }
        }

        public async Task<int> GetConnectedEdgeDevicesCount()
        {
            try
            {
                var count = await this.registryManager
                    .CreateQuery("SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND connectionState = 'Connected'")
                    .GetNextAsJsonAsync();

                return !JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result) ? 0 : result.Value<int>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get connected edge devices count", e);
            }
        }

        public async Task<int> GetConcentratorsCount()
        {
            try
            {
                var count = await this.registryManager
                    .CreateQuery("SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND devices.tags.deviceType = 'LoRa Concentrator'")
                    .GetNextAsJsonAsync();

                return !JObject.Parse(count.Single()).TryGetValue("totalNumber", out var result) ? 0 : result.Value<int>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get connected LoRaWAN concentrators count", e);
            }
        }

        public async Task<DeviceCredentials> GetDeviceCredentials(IDeviceDetails device)
        {
            try
            {
                var model = await this.deviceModelRepository.GetByIdAsync(device.ModelId);

                return await this.deviceRegistryProvider.GetEnrollmentCredentialsAsync(device.DeviceID, model.Id);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get model {device.ModelName}: {e.Message}", e);
            }
        }

        /// <summary>
        /// Gets the IoT Edge device enrollement credentials.
        /// </summary>
        /// <param name="edgeDeviceId">the edge device id.</param>
        /// <returns>Enrollment credentials.</returns>
        /// <exception cref="ResourceNotFoundException"></exception>
        public async Task<DeviceCredentials> GetEdgeDeviceCredentials(IoTEdgeDevice device)
        {
            var deviceTwin = await GetDeviceTwin(device.DeviceId);

            if (deviceTwin == null)
            {
                throw new ResourceNotFoundException($"IoT Edge {device.DeviceId} doesn't exist.");
            }

            var modelId = DeviceHelper.RetrieveTagValue(deviceTwin, nameof(IoTEdgeDevice.ModelId));

            return await this.deviceRegistryProvider.GetEnrollmentCredentialsAsync(device.DeviceId, modelId);
        }

        /// <summary>
        /// Retrieves the last configuration of the IoT Edge.
        /// </summary>
        /// <param name="twin">The twin.</param>
        public async Task<ConfigItem> RetrieveLastConfiguration(IoTEdgeDevice ioTEdgeDevice)
        {
            var twin = await this.registryManager.GetTwinAsync(ioTEdgeDevice.DeviceId);
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

        public async Task<Twin> CreateNewTwinFromDeviceId(string deviceId)
        {
            var existingDevice = await this.GetDevice(deviceId);
            if (existingDevice != null)
            {
                throw new DeviceAlreadyExistsException($"The device with ID {deviceId} already exists");
            }

            // Create a new Twin from the form's fields.
            return new Twin
            {
                DeviceId = deviceId
            };
        }

        public async Task<List<string>> GetAllGatewayID()
        {
            try
            {
                var query = this.registryManager.CreateQuery($"SELECT DeviceID FROM devices.modules WHERE devices.modules.moduleId = 'LoRaWanNetworkSrvModule'");
                var list = new List<string>();

                var value = (await query.GetNextAsJsonAsync()).ToList();
                list.AddRange(value.Select(x => JObject.Parse(x)["deviceId"]?.ToString()));

                return list;
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Failed to parse device identifier", e);
            }
        }

        public async Task<IEnumerable<string>> GetDevicesToExport()
        {
            try
            {
                var filter = "WHERE (NOT IS_DEFINED (tags.deviceType) OR tags.deviceType <> 'LoRa Concentrator') AND (capabilities.iotEdge = false)";
                var query = this.registryManager.CreateQuery($"SELECT deviceId, tags, properties.desired FROM devices { filter }");
                var response = await query.GetNextAsJsonAsync();
                return response;
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Failed to query devices to export", e);
            }
        }

        public Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateEnrollementScript(string template, IoTEdgeDevice device)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateEdgeDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDeviceCredentials(IoTEdgeDevice device)
        {
            throw new NotImplementedException();
        }

        public Task<bool?> IsEdgeDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Amazon.IoT.Model.DescribeThingResponse>> GetAllThing()
        {
            throw new NotImplementedException();
        }
    }
}
