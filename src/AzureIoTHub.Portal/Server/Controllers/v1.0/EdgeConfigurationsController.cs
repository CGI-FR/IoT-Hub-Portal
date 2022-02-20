// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Interfaces;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Newtonsoft.Json.Linq;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/edge/configurations")]
    [ApiExplorerSettings(GroupName = "IoT Edge")]
    public class EdgeConfigurationsController : ControllerBase
    {
        private readonly IConfigs configService;

        public EdgeConfigurationsController(IConfigs configService)
        {
            this.configService = configService;
        }

        /// <summary>
        /// Gets the IoT Edge deployment configurations.
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GET IoT Edge config list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                var moduleList = new List<IoTEdgeModule>();

                // Only deployments have modules. If it doesn't, it's a configuration and we don't want to keep it.
                if (config.Content.ModulesContent != null)
                {
                    moduleList.AddRange(config.Content.ModulesContent.Select(x => new IoTEdgeModule
                    {
                        ModuleName = x.Key
                    }));

                    ConfigListItem result = ConfigHelper.CreateConfigListItem(config, moduleList);
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the specified configuration.
        /// </summary>
        /// <param name="configurationID">The configuration identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Could not parse properties.desired.</exception>
        [HttpGet("{configurationID}", Name ="GET IoT Edge configuration")]
        public async Task<ConfigListItem> Get(string configurationID)
        {
            var config = await this.configService.GetConfigItem(configurationID);
            var moduleList = new List<IoTEdgeModule>();

            // Details of every modules are stored within the EdgeAgent module data
            if (config.Content.ModulesContent != null
                && config.Content.ModulesContent.TryGetValue("$edgeAgent", out IDictionary<string, object> edgeAgentModule)
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
                        IoTEdgeModule newModule = ConfigHelper.CreateGatewayModule(config, m);
                        moduleList.Add(newModule);
                    }
                }

                // Adds system modules to the list of modules
                if (modObject.TryGetValue("systemModules", out JToken systemModulesToken))
                {
                    foreach (var sm in systemModulesToken.Values<JProperty>())
                    {
                        IoTEdgeModule newModule = ConfigHelper.CreateGatewayModule(config, sm);
                        moduleList.Add(newModule);
                    }
                }
            }

            ConfigListItem result = ConfigHelper.CreateConfigListItem(config, moduleList);
            return result;
        }
    }
}
