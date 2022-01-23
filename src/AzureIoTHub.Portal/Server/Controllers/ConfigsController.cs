// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class ConfigsController : ControllerBase
    {
        private readonly ILogger<ConfigsController> logger;

        private readonly RegistryManager registryManager;
        private readonly ConfigsServices configService;

        public ConfigsController(
            ILogger<ConfigsController> logger,
            ConfigsServices configService,
            RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.configService = configService;
        }

        /// <summary>
        /// Gets a list of deployments as ConfigListItem from Azure IoT Hub.
        /// </summary>
        /// <returns>A list of ConfigListItem.</returns>
        [HttpGet]
        public async Task<IEnumerable<ConfigListItem>> Get()
        {
            // Retrieve every Configurations, regardless of the parameter given... Why?
            // TODO : Check & fix this
            // List<Configuration> configList = await this.registryManager.GetConfigurationsAsync(0) as List<Configuration>;
            List<Configuration> configList = await this.configService.GetAllConfigs() as List<Configuration>;
            var results = new List<ConfigListItem>();

            // Azure Configurations may have different types: "Configuration", "Deployment" or "LayeredDeployment"
            foreach (Configuration config in configList)
            {
                List<GatewayModule> moduleList = new ();

                // Only deployments have modules. If it doesn't, it's a configuration and we don't want to keep it.
                if (config.Content.ModulesContent != null)
                {
                    // Retrieve the name of each module of this deployment
                    foreach (var module in config.Content.ModulesContent)
                    {
                        var newModule = new GatewayModule
                        {
                            ModuleName = module.Key
                        };
                        moduleList.Add(newModule);
                    }

                    ConfigListItem result = ConfigHelper.CreateConfigListItem(config, moduleList);
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieve a specific deployment and its modules from the IoT Hub.
        /// Converts it to a ConfigListItem.
        /// </summary>
        /// <param name="configurationID">ID of the deployment to retrieve.</param>
        /// <returns>The ConfigListItem corresponding to the given ID.</returns>
        [HttpGet("{configurationID}")]
        public async Task<ConfigListItem> Get(string configurationID)
        {
            var config = await this.configService.GetConfigItem(configurationID);
            List<GatewayModule> moduleList = new ();
            if (config.Content.ModulesContent != null)
            {
                // Details of every modules are stored within the EdgeAgent module data
                if (config.Content.ModulesContent.ContainsKey("$edgeAgent"))
                {
                    if (config.Content.ModulesContent["$edgeAgent"].ContainsKey("properties.desired"))
                    {
                        // Converts the object to a JObject to access its properties more easily
                        JObject modObject = config.Content.ModulesContent["$edgeAgent"]["properties.desired"] as JObject;

                        // Adds regular modules to the list of modules
                        if (modObject.ContainsKey("modules"))
                        {
                            // Converts it to a JObject to be able to iterate through it
                            JObject modules = modObject["modules"] as JObject;
                            foreach (var m in modules)
                            {
                                GatewayModule newModule = ConfigHelper.CreateGatewayModule(config, m);
                                moduleList.Add(newModule);
                            }
                        }

                        // Adds system modules to the list of modules
                        if (modObject.ContainsKey("systemModules"))
                        {
                            // Converts it to a JObject to be able to iterate through it
                            JObject systemModules = modObject["systemModules"] as JObject;
                            foreach (var sm in systemModules)
                            {
                                GatewayModule newModule = ConfigHelper.CreateGatewayModule(config, sm);
                                moduleList.Add(newModule);
                            }
                        }
                    }
                }
            }

            ConfigListItem result = ConfigHelper.CreateConfigListItem(config, moduleList);
            return result;
        }
    }
}
