// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    [ApiController]
    [Route("api/[controller]")]
    public class ConfigsController : ControllerBase
    {
        private readonly ConfigsServices configService;

        public ConfigsController(ConfigsServices configService)
        {
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
            var configList = await this.configService.GetAllConfigs();
            var results = new List<ConfigListItem>();

            // Azure Configurations may have different types: "Configuration", "Deployment" or "LayeredDeployment"
            foreach (Configuration config in configList)
            {
                var moduleList = new List<GatewayModule>();

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
            var moduleList = new List<GatewayModule>();
            if (config.Content.ModulesContent != null)
            {
                // Details of every modules are stored within the EdgeAgent module data
                if (config.Content.ModulesContent.TryGetValue("$edgeAgent", out IDictionary<string, object> edgeAgentModule)
                    && edgeAgentModule.TryGetValue("properties.desired", out object edgeAgentDesiredProperties))
                {
                    // Converts the object to a JObject to access its properties more easily
                    JObject modObject = edgeAgentDesiredProperties as JObject;

                    if (modObject == null)
                    {
                        throw new InvalidOperationException("Could not parse properties.desired.");
                    }

                    // Adds regular modules to the list of modules
                    if (modObject.TryGetValue("modules", out JToken modules))
                    {
                        foreach (var m in modules.Values<JProperty>())
                        {
                            GatewayModule newModule = ConfigHelper.CreateGatewayModule(config, m);
                            moduleList.Add(newModule);
                        }
                    }

                    // Adds system modules to the list of modules
                    if (modObject.TryGetValue("systemModules", out JToken systemModulesToken))
                    {
                        foreach (var sm in systemModulesToken.Values<JProperty>())
                        {
                            GatewayModule newModule = ConfigHelper.CreateGatewayModule(config, sm);
                            moduleList.Add(newModule);
                        }
                    }
                }
            }

            ConfigListItem result = ConfigHelper.CreateConfigListItem(config, moduleList);
            return result;
        }
    }
}
