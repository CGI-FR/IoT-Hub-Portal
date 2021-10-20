// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

        private readonly RegistryManager registryManager;

        public DevicesController(ILogger<DevicesController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
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
                ModelType = RetrieveTagValue(item, "modelType")
            };
            return result;
        }

        /// <summary>
        /// Creates, updates or deletes a device, depending on the way the function was triggered.
        /// </summary>
        /// <param name="device">Device to create/update/delete.</param>
        /// <param name="actionToPerform">Specific action to perform: create/update/delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [HttpPost("{actionToPerform}")]
        public async Task Post(DeviceListItem device, string actionToPerform)
        {
            if (actionToPerform == "delete")
            {
                // TODO : Deal with success/failure status
                await this.registryManager.RemoveDeviceAsync(device.DeviceID);
            }
            else
            {
                if (actionToPerform == "create")
                {
                    // Create a new Twin from the fields from the form.
                    Twin newTwin = new () { DeviceId = device.DeviceID };
                    newTwin.Tags["locationCode"] = device.LocationCode;
                    newTwin.Tags["deviceType"] = device.DeviceType;
                    newTwin.Tags["modelType"] = device.ModelType;
                    newTwin.Tags["assetID"] = device.AssetID;
                    newTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                    newTwin.Properties.Desired["AppKey"] = device.AppKey;

                    // TODO : Deal with success/failure state
                    try
                    {
                        await this.registryManager.AddDeviceWithTwinAsync(new Device(device.DeviceID), newTwin);
                    }

                    // Supposedly throws a exception if the device already exists within the hub
                    // TODO : Check if it works properly
                    catch (DeviceAlreadyExistsException e)
                    {
                        Console.WriteLine($"ERROR: {e}");
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

                    // Update the twin
                    Twin twin = await this.registryManager.ReplaceTwinAsync(device.DeviceID, currentTwin, currentTwin.ETag);
                }

                // Device status (enabled/disabled) has to be dealt with afterwards
                Device currentDevice = await this.registryManager.GetDeviceAsync(device.DeviceID);
                // Sets the current Device status according to the value entered in the form
                currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                // Update the device on the hub
                await this.registryManager.UpdateDeviceAsync(currentDevice);
            }
        }
    }
}
