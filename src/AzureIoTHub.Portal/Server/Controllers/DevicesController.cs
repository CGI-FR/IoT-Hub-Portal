// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
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

        [HttpPost("{isNew}")]
        public async Task<DeviceListItem> Post(DeviceListItem device, bool isNew)
        {
            // await Task.Delay(1);
            // var test = new Device();
            // await this.registryManager.AddDeviceAsync(test);
            if (isNew)
            {
                Device newDevice = new Device();

                Twin newTwin = new Twin
                {
                    DeviceId = "newDevice"
                };

                await this.registryManager.AddDeviceWithTwinAsync(newDevice, newTwin);

                var test = new DeviceListItem();
                Console.WriteLine($"New device! {isNew.ToString()}");
                return test;
            }
            else
            {
                Twin currentTwin = await this.registryManager.GetTwinAsync(device.DeviceID);

                // await this.registryManager.ReplaceTwinAsync(device.DeviceID, newTwin, "etag");
                var item = await this.registryManager.GetTwinAsync(device.DeviceID);

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

                Console.WriteLine($"Test? {isNew.ToString()}");
                Console.WriteLine(result.AppEUI);
                return result;
            }
        }
    }
}
