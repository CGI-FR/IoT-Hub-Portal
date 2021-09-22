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
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<DevicesController> logger;

        private readonly RegistryManager registryManager;

        public DevicesController(ILogger<DevicesController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
        }

        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> Get()
        {
            var query = this.registryManager.CreateQuery("SELECT * FROM devices WHERE devices.capabilities.iotEdge = false");

            var items = await query.GetNextAsTwinAsync();

            var results = new List<DeviceListItem>();

            foreach (var item in items)
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

        private static string RetrieveTagValue(Twin item, string tagName)
        {
            if (item.Tags.Contains(tagName))
                return item.Tags[tagName];
            else
                return "mock_" + tagName;
        }

        private static string RetrievePropertyValue(Twin item, string propertyName)
        {
            if (item.Properties.Desired.Contains(propertyName))
                return item.Properties.Desired[propertyName];
            else
                return "mock_" + propertyName;
        }

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

        [HttpPost("{actionToPerform}")]
        public async Task Post(DeviceListItem device, string actionToPerform)
        {
            if (actionToPerform == "delete")
            {
                await this.registryManager.RemoveDeviceAsync(device.DeviceID);
            }
            else
            {
                if (actionToPerform == "create")
                {
                    Twin newTwin = new Twin { DeviceId = device.DeviceID };

                    newTwin.Tags["locationCode"] = device.LocationCode;
                    newTwin.Tags["deviceType"] = device.DeviceType;
                    newTwin.Tags["modelType"] = device.ModelType;
                    newTwin.Tags["assetID"] = device.AssetID;
                    newTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                    newTwin.Properties.Desired["AppKey"] = device.AppKey;

                    try
                    {
                        await this.registryManager.AddDeviceWithTwinAsync(new Device(device.DeviceID), newTwin);
                    }
                    catch (DeviceAlreadyExistsException)
                    {
                        // newDevice = await this.registryManager.GetDeviceAsync(deviceId);
                        Console.WriteLine("ERROR");
                    }

                    // Console.WriteLine("Generated device key: {0}", newDevice.Authentication.SymmetricKey.PrimaryKey);
                }

                if (actionToPerform == "update")
                {
                    Twin currentTwin = await this.registryManager.GetTwinAsync(device.DeviceID);
                    currentTwin.Tags["locationCode"] = device.LocationCode;
                    currentTwin.Tags["assetID"] = device.AssetID;
                    currentTwin.Properties.Desired["AppEUI"] = device.AppEUI;
                    currentTwin.Properties.Desired["AppKey"] = device.AppKey;
                    Twin twin = await this.registryManager.ReplaceTwinAsync(device.DeviceID, currentTwin, currentTwin.ETag);
                }

                // Statut du device (enabled/disabled)
                Device currentDevice = await this.registryManager.GetDeviceAsync(device.DeviceID);
                currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                await this.registryManager.UpdateDeviceAsync(currentDevice);
            }
        }
    }
}
