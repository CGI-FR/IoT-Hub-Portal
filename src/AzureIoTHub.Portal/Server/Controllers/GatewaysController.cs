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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class GatewaysController : ControllerBase
    {
        private readonly ILogger<GatewaysController> logger;

        private readonly RegistryManager registryManager;

        public GatewaysController(ILogger<GatewaysController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
        }

        /// <summary>
        /// Fonction permettant de récupèrer la liste des appareils Edge .
        /// Après avoir éxecuté la query du registryManager on récupère le resultat
        /// sous la forme d'une liste de Twin.
        /// </summary>
        /// <returns>Retourne un IEnumerable de GatewayListItem avec les propriètés que l'on souhaite.</returns>
        [HttpGet]
        public async Task<IEnumerable<GatewayListItem>> Get()
        {
            var query = this.registryManager.CreateQuery("SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId", 10);
            var query2 = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);
            List<GatewayListItem> gatewayList = new List<GatewayListItem>();

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                var pageBis = await query2.GetNextAsTwinAsync();
                int index = 0;

                foreach (var twin in page)
                {
                    var result = new GatewayListItem
                    {
                        DeviceId = twin.DeviceId,
                        Status = twin.Status.Value.ToString(),
                        NbDevices = 0,
                        Type = "unknow"
                    };

                    if (twin.Properties.Reported.Contains("clients"))
                    {
                        result.NbDevices = twin.Properties.Reported["clients"].Count;
                    }

                    if (pageBis.ElementAt(index).Tags.Contains("purpose"))
                    {
                        result.Type = pageBis.ElementAt(index).Tags["purpose"];
                    }

                    gatewayList.Add(result);
                    index++;
                }
            }

            return gatewayList;
        }

        [HttpGet("deviceID")]
        public async Task<Gateway> Get(string deviceID)
        {
            var device = await this.registryManager.GetTwinAsync(deviceID);
            return new Gateway
            {
                DeviceId = device.DeviceId,
                Status = device.Status.Value.ToString(),
                Type = device.Tags["purpose"]
            };
        }
    }
}
