// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]

    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> logger;
        private readonly ServiceClient serviceClient;
        private readonly HttpClient http;
        private readonly DevicesServices devicesService;
        private readonly RegistryManager registryManager;
        private readonly ITableClientFactory tableClientFactory;
        private readonly ISensorImageManager sensorImageManager;

        public DevicesController(
            ILogger<DevicesController> logger,
            ITableClientFactory tableClientFactory,
            ISensorImageManager sensorImageManager,
            RegistryManager registryManager,
            ServiceClient serviceClient,
            DevicesServices devicesService,
            HttpClient http)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.serviceClient = serviceClient;
            this.http = http;
            this.devicesService = devicesService;
            this.tableClientFactory = tableClientFactory;
            this.sensorImageManager = sensorImageManager;
        }

        /// <summary>
        /// Gets a list of devices as DeviceListItem from Azure IoT Hub.
        /// Fields that do not appear in the device list are not defined here.
        /// </summary>
        /// <returns>A list of DeviceListItem.</returns>
        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> Get()
        {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice();
            var results = new List<DeviceListItem>();

            // Convert each Twin to a DeviceListItem with specific fields
            foreach (Twin item in items)
            {
                var result = new DeviceListItem
                {
                    DeviceID = item.DeviceId,
                    ImageUrl = await this.sensorImageManager.GetSensorImageUriAsync(item.ModelId, false),
                    IsConnected = item.ConnectionState == DeviceConnectionState.Connected,
                    IsEnabled = item.Status == DeviceStatus.Enabled,
                    LastActivityDate = item.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
                    AppEUI = Helpers.DeviceHelper.RetrievePropertyValue(item, "AppEUI"),
                    AppKey = Helpers.DeviceHelper.RetrievePropertyValue(item, "AppKey"),
                    LocationCode = Helpers.DeviceHelper.RetrieveTagValue(item, "locationCode")
                };

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Retrieve a specific device and from the IoT Hub.
        /// Converts it to a DeviceListItem.
        /// </summary>
        /// <param name="deviceID">ID of the device to retrieve.</param>
        /// <returns>The DeviceListItem corresponding to the given ID.</returns>
        [HttpGet("{deviceID}")]
        public async Task<DeviceListItem> Get(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            var result = new DeviceListItem
            {
                DeviceID = item.DeviceId,
                ModelType = item.ModelId,
                ImageUrl = await this.sensorImageManager.GetSensorImageUriAsync(item.ModelId),
                IsConnected = item.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = item.Status == DeviceStatus.Enabled,
                LastActivityDate = item.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
                AppEUI = Helpers.DeviceHelper.RetrievePropertyValue(item, "AppEUI"),
                AppKey = Helpers.DeviceHelper.RetrievePropertyValue(item, "AppKey"),
                LocationCode = Helpers.DeviceHelper.RetrieveTagValue(item, "locationCode"),
                AssetID = Helpers.DeviceHelper.RetrieveTagValue(item, "assetID"),
                DeviceType = Helpers.DeviceHelper.RetrieveTagValue(item, "deviceType"),
                Commands = this.RetrieveCommands(item.ModelId)
            };

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(DeviceListItem device)
        {
            try
            {
                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceID,
                    ModelId = device.ModelType
                };

                newTwin.Tags["locationCode"] = device.LocationCode;
                newTwin.Tags["deviceType"] = device.DeviceType;
                newTwin.Tags["assetID"] = device.AssetID;
                newTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                newTwin.Properties.Desired["AppKey"] = device.AppKey;

                DeviceStatus status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// this function update the twin and the device.
        /// </summary>
        /// <param name="device">the device object.</param>
        /// <returns>the update twin.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateDeviceAsync(DeviceListItem device)
        {
            try
            {
                // Get the current twin from the hub, based on the device ID
                Twin currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceID);

                // Update the twin properties
                currentTwin.Tags["locationCode"] = device.LocationCode;
                currentTwin.Tags["assetID"] = device.AssetID;
                currentTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                currentTwin.Properties.Desired["AppKey"] = device.AppKey;

                Twin newTwin = await this.devicesService.UpdateDeviceTwin(device.DeviceID, currentTwin);

                // Device status (enabled/disabled) has to be dealt with afterwards
                Device currentDevice = await this.devicesService.GetDevice(device.DeviceID);

                // Sets the current Device status according to the value entered in the form
                currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                _ = await this.devicesService.UpdateDevice(currentDevice);

                return this.Ok(newTwin);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
        }

        /// <summary>
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceID">the device id.</param>
        /// <returns>ok status on success.</returns>
        [HttpDelete("{deviceID}")]
        public async Task<IActionResult> Delete(string deviceID)
        {
            try
            {
                await this.devicesService.DeleteDevice(deviceID);
                return this.Ok("device delete !");
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
        }

        /// <summary>
        /// Permit to execute cloud to device message.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <param name="command">the command who contain the name and the trame.</param>
        /// <returns>a CloudToDeviceMethodResult .</returns>
        [HttpPost("{deviceId}/{methodName}")]
        public async Task<IActionResult> ExecuteLoraMethod(string deviceId, SensorCommand command)
        {
            try
            {
                JsonContent commandContent = JsonContent.Create(new
                {
                    rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Trame)),
                    fport = command.Port
                });

                commandContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var result = await this.devicesService.ExecuteLoraMethod(deviceId, commandContent);

                this.logger.LogInformation($"{result.Content}");

                return this.Ok(await result.Content.ReadFromJsonAsync<dynamic>());
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieve all the commands of a device.
        /// </summary>
        /// <param name="model_type"> the model type of the device.</param>
        /// <returns>Corresponding list of commands or an empty list if it doesn't have any command.</returns>
        private List<SensorCommand> RetrieveCommands(string model_type)
        {
            var commands = new List<SensorCommand>();

            if (model_type == "undefined_modelType")
            {
                return commands;
            }

            Pageable<TableEntity> queryResultsFilter = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey  eq '{model_type}'");

            foreach (TableEntity qEntity in queryResultsFilter)
            {
                commands.Add(
                    new SensorCommand()
                    {
                        Name = qEntity.RowKey,
                        Trame = qEntity.GetString("Trame"),
                        Port = (int)qEntity["Port"]
                    });
            }

            return commands;
        }
    }
}
