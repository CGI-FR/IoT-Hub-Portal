// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Azure.Data.Tables.Models;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
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

        private readonly RegistryManager registryManager;

        public DevicesController(
            ILogger<DevicesController> logger,
            RegistryManager registryManager,
            ServiceClient serviceClient,
            TableClient tableClient)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.tableClient = tableClient;
            this.serviceClient = serviceClient;
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
                    AppEUI = RetrievePropertyValue(item, "AppEUI"),
                    AppKey = RetrievePropertyValue(item, "AppKey"),
                    LocationCode = RetrieveTagValue(item, "locationCode")
                };

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Checks if the specific tag exists within the device twin,
        /// Returns the corresponding value if so, else returns a generic value "undefined".
        /// </summary>
        /// <param name="item">Device twin.</param>
        /// <param name="tagName">Tag to retrieve.</param>
        /// <returns>Corresponding tag value, or "undefined" if it doesn't exist.</returns>
        private static string RetrieveTagValue(Twin item, string tagName)
        {
            if (item.Tags.Contains(tagName))
                return item.Tags[tagName];
            else
                return "undefined_" + tagName;
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns a generic value "undefined".
        /// </summary>
        /// <param name="item">Device twin.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>Corresponding property value, or "undefined" if it doesn't exist.</returns>
        private static string RetrievePropertyValue(Twin item, string propertyName)
        {
            if (item.Properties.Desired.Contains(propertyName))
                return item.Properties.Desired[propertyName];
            else
                return "undefined_" + propertyName;
        }

        /// <summary>
        /// Retrieve all the commands of a device.
        /// </summary>
        /// <param name="model_type"> the model type of the device.</param>
        /// <returns>Corresponding list of commands or an empty list if it doesn't have any command.</returns>
        private List<SensorCommand> RetrieveCommands(string model_type)
        {
            List<SensorCommand> commands = new List<SensorCommand>();

            if (model_type == "undefined_modelType")
            {
                // Pageable<TableEntity> queryResultsFilter = this.tableClient.Query<TableEntity>(filter: $"PartitionKey  eq '{model_type}'");
                Pageable<TableEntity> queryResultsFilter = this.tableClient.Query<TableEntity>(filter: $"PartitionKey  eq 'sensor_model01'");
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
                AppEUI = RetrievePropertyValue(item, "AppEUI"),
                AppKey = RetrievePropertyValue(item, "AppKey"),
                LocationCode = RetrieveTagValue(item, "locationCode"),
                AssetID = RetrieveTagValue(item, "assetID"),
                DeviceType = RetrieveTagValue(item, "deviceType"),
                ModelType = RetrieveTagValue(item, "modelType"),
                Commands = this.RetrieveCommands(RetrieveTagValue(item, "modelType"))
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
                CloudToDeviceMethod method = new CloudToDeviceMethod(command.Name);
                string commandPayload = $"{{\"Trame\":\"{command.Trame}\", \"Port\":\"{command.Port}\"}}";
                method.SetPayloadJson(commandPayload);

                CloudToDeviceMethodResult result = await this.serviceClient.InvokeDeviceMethodAsync(deviceId, method);
                this.logger.LogInformation($"iot hub device : {deviceId} execute methode {command.Name}.");

                if (result is null)
                {
                    throw new Exception($"Command {command.Name} invocation returned null");
                }

                this.logger.LogInformation($"iot hub device {method}: {result.GetPayloadAsJson()} ");

                return this.Ok(result);
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
    }
}
