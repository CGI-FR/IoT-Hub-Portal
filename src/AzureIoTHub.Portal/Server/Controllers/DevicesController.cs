// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Azure.Data.Tables.Models;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]

    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> logger;
        private readonly TableClient tableClient;
        private readonly ServiceClient serviceClient;
        private readonly HttpClient http;
        private readonly IConfiguration configuration;
        private readonly DevicesServices devicesService;

        private readonly RegistryManager registryManager;

        public DevicesController(
            IConfiguration configuration,
            ILogger<DevicesController> logger,
            RegistryManager registryManager,
            ServiceClient serviceClient,
            DevicesServices devicesService,
            HttpClient http,
            TableClient tableClient)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.tableClient = tableClient;
            this.serviceClient = serviceClient;
            this.http = http;
            this.configuration = configuration;
            this.devicesService = devicesService;
        }

        /// <summary>
        /// Gets a list of devices as DeviceListItem from Azure IoT Hub.
        /// Fields that do not appear in the device list are not defined here.
        /// </summary>
        /// <returns>A list of DeviceListItem.</returns>
        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> Get()
        {
            // Query to retrieve every devices from Azure IoT Hub, apart from Edge devices (devices.capabilities.iotEdge = false)
            var query = this.registryManager.CreateQuery("SELECT * FROM devices WHERE devices.capabilities.iotEdge = false");

            // Gets all the twins from this devices
            IEnumerable<Twin> items = await query.GetNextAsTwinAsync();
            List<DeviceListItem> results = new ();

            // Convert each Twin to a DeviceListItem with specific fields
            foreach (Twin item in items)
            {
                var result = new DeviceListItem
                {
                    DeviceID = item.DeviceId,
                    IsConnected = item.ConnectionState == DeviceConnectionState.Connected,
                    IsEnabled = item.Status == DeviceStatus.Enabled,
                    LastActivityDate = item.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
                    AppEUI = Helpers.Helpers.RetrievePropertyValue(item, "AppEUI"),
                    AppKey = Helpers.Helpers.RetrievePropertyValue(item, "AppKey"),
                    LocationCode = Helpers.Helpers.RetrieveTagValue(item, "locationCode")
                };

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Retrieve all the commands of a device.
        /// </summary>
        /// <param name="model_type"> the model type of the device.</param>
        /// <returns>Corresponding list of commands or an empty list if it doesn't have any command.</returns>
        private List<SensorCommand> RetrieveCommands(string model_type)
        {
            List<SensorCommand> commands = new List<SensorCommand>();

            if (model_type != "undefined_modelType")
            {
                Pageable<TableEntity> queryResultsFilter = this.tableClient.Query<TableEntity>(filter: $"PartitionKey  eq '{model_type}'");

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
            }

            return commands;
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
            var item = await this.registryManager.GetTwinAsync(deviceID);

            var result = new DeviceListItem
            {
                DeviceID = item.DeviceId,
                IsConnected = item.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = item.Status == DeviceStatus.Enabled,
                LastActivityDate = item.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
                AppEUI = Helpers.Helpers.RetrievePropertyValue(item, "AppEUI"),
                AppKey = Helpers.Helpers.RetrievePropertyValue(item, "AppKey"),
                LocationCode = Helpers.Helpers.RetrieveTagValue(item, "locationCode"),
                AssetID = Helpers.Helpers.RetrieveTagValue(item, "assetID"),
                DeviceType = Helpers.Helpers.RetrieveTagValue(item, "deviceType"),
                ModelType = Helpers.Helpers.RetrieveTagValue(item, "modelType"),
                Commands = this.RetrieveCommands(Helpers.Helpers.RetrieveTagValue(item, "modelType"))
            };
            return result;
        }

        /// <summary>
        /// Permit to execute cloud to device message.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <param name="command">the command who contain the name and the trame.</param>
        /// <returns>a CloudToDeviceMethodResult .</returns>
        [HttpPost("{deviceId}/{methodName}")]
        public async Task<IActionResult> ExecuteMethode(string deviceId, SensorCommand command)
        {
            try
            {
                JsonContent commandContent = JsonContent.Create(new
                {
                    rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Trame)),
                    fport = command.Port
                });

                commandContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var result = await this.http.PostAsync($"{this.configuration["IoTAzureFunction:url"]}/{deviceId}{this.configuration["IoTAzureFunction:code"]}", commandContent);

                this.logger.LogInformation($"{result.Content}");

                return this.Ok(await result.Content.ReadFromJsonAsync<dynamic>());
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Creates, updates or deletes a device, depending on the way the function was triggered.
        /// </summary>
        /// <param name="device">Device to create/update/delete.</param>
        /// <param name="actionToPerform">Specific action to perform: create/update/delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [HttpPost("{actionToPerform}")]
        // public async Task<HttpResponseMessage> Post(DeviceListItem device, string actionToPerform)
        public async Task<IActionResult> Post(DeviceListItem device, string actionToPerform)
        {
            if (actionToPerform == "delete")
            {
                // TODO : Deal more effectively with success/failure status
                try
                {
                    await this.registryManager.RemoveDeviceAsync(device.DeviceID);
                }
                catch (Exception e)
                {
                    return this.Problem($"Error while deleting device : {e.Message}", statusCode: 418);
                }
            }
            else
            {
                if (actionToPerform == "create")
                {
                    // Create a new Twin from the form's fields.
                    Twin newTwin = new () { DeviceId = device.DeviceID };
                    newTwin.Tags["locationCode"] = device.LocationCode;
                    newTwin.Tags["deviceType"] = device.DeviceType;
                    newTwin.Tags["modelType"] = device.ModelType;
                    newTwin.Tags["assetID"] = device.AssetID;
                    newTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                    newTwin.Properties.Desired["AppKey"] = device.AppKey;

                    // TODO : Deal more effectively with success/failure status
                    try
                    {
                        var response = await this.registryManager.AddDeviceWithTwinAsync(new Device(device.DeviceID), newTwin);

                        // Manually throw an exception if something went wrong while creating the device
                        if (!response.IsSuccessful)
                        {
                            throw new Exception(response.Errors.First().ErrorStatus);
                        }
                    }
                    catch (Exception e)
                    {
                        return this.Problem($"Error while creating device : {e.Message}", statusCode: 418);
                    }
                }

                if (actionToPerform == "update")
                {
                    // Get the current twin from the hub, based on the device ID
                    Twin currentTwin = await this.registryManager.GetTwinAsync(device.DeviceID);

                    // Update the twin properties
                    currentTwin.Tags["locationCode"] = device.LocationCode;
                    currentTwin.Tags["assetID"] = device.AssetID;
                    currentTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                    currentTwin.Properties.Desired["AppKey"] = device.AppKey;

                    // TODO : Deal more effectively with success/failure status
                    try
                    {
                        Twin twin = await this.registryManager.ReplaceTwinAsync(device.DeviceID, currentTwin, currentTwin.ETag);
                    }
                    catch (Exception e)
                    {
                        return this.Problem($"Error while replacing twin : {e.Message}", statusCode: 418);
                    }
                }

                // Device status (enabled/disabled) has to be dealt with afterwards
                Device currentDevice = await this.registryManager.GetDeviceAsync(device.DeviceID);
                // Sets the current Device status according to the value entered in the form
                currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                // Update the device on the hub
                try
                {
                    await this.registryManager.UpdateDeviceAsync(currentDevice);
                }
                catch (Exception e)
                {
                    return this.Problem($"Error while updating device : {e.Message}", statusCode: 418);
                }
            }

            return this.Ok("Everything went well, yay !");
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(DeviceListItem device)
        {
            try
            {
                // Create a new Twin from the form's fields.
                Twin newTwin = new () { DeviceId = device.DeviceID };
                newTwin.Tags["locationCode"] = device.LocationCode;
                newTwin.Tags["deviceType"] = device.DeviceType;
                newTwin.Tags["modelType"] = device.ModelType;
                newTwin.Tags["assetID"] = device.AssetID;
                newTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                newTwin.Properties.Desired["AppKey"] = device.AppKey;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin);

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
    }
}
