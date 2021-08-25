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
            var rng = new Random();

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
                    AppEUI = "AppEUI",
                    AppKey = "AppKey",
                    LocationCode = "Location"
                };

                if (item.Properties.Desired.Contains("AppEUI"))
                    result.AppEUI = item.Properties.Desired["AppEUI"];

                if (item.Properties.Desired.Contains("AppKey"))
                    result.AppKey = item.Properties.Desired["AppKey"];

                if (item.Tags.Contains("locationCode"))
                    result.LocationCode = item.Tags["locationCode"];

                results.Add(result);
            }

            return results;

            // return items.Select(c => new DeviceListItem
            // {
            //    DeviceID = c.DeviceId,
            //    IsConnected = c.ConnectionState == DeviceConnectionState.Connected,
            //    IsEnabled = c.Status == DeviceStatus.Enabled,
            //    LastActivityDate = c.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
            //    Tags = c.Tags.ToJson()
            //    // Tags = c.Properties.Desired[""]
            // });
        }
    }
}
