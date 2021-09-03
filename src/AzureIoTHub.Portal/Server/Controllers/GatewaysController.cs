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

        [HttpGet("{deviceId}")]
        public async Task<Gateway> Get(string deviceId)
        {
            var device = await this.registryManager.GetTwinAsync(deviceId);
            var query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");
            var query2 = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']");
            var deviceWithModules = await query.GetNextAsTwinAsync();
            var deviceWithClient = await query2.GetNextAsTwinAsync();
            // var test = this.registryManager.GetConfigurationAsync();
            Gateway gateway = new Gateway();

            gateway.DeviceId = device.DeviceId;
            gateway.Status = device.Status.Value.ToString();
            if (device.Tags.Contains("purpose"))
            {
                gateway.Type = device.Tags["purpose"];
            }
            else
            {
                gateway.Type = "unknow";
            }

            if (device.Tags.Contains("env"))
            {
                gateway.Environement = device.Tags["env"];
            }
            else
            {
                gateway.Environement = "unknow";
            }

            // récupération des informations sur les clients connceté à la gateway
            foreach (var item in deviceWithClient)
            {
                if (item.Properties.Reported.Contains("clients") && item.DeviceId == deviceId)
                {
                    gateway.NbDevices = item.Properties.Reported["clients"].Count;
                }
            }

            // récupération des informations sur le modules de la gateways
            foreach (var item in deviceWithModules)
            {
                if (item.Properties.Desired.Contains("modules") && item.DeviceId == deviceId)
                {
                    gateway.NbModule = item.Properties.Desired["modules"].Count;
                }

                if (gateway.NbModule > 0)
                {
                    foreach (var element in item.Properties.Reported["modules"])
                    {
                        var module = new GatewayModule();
                        module.ModuleName = element.Key;
                        if (element.Value.Contains("status"))
                        {
                            module.Status = element.Value["status"];
                        }

                        if (element.Value.Contains("version"))
                        {
                            module.Version = element.Value["version"];
                        }

                        gateway.Modules.Add(module);
                        // gateway.Modules.Add(new GatewayModule
                        // {
                        //    // ModuleName = element,
                        //    Version = element["version"],
                        //    Status = element["status"]
                        // });
                    }
                }
            }

            return gateway;
        }
    }
}
