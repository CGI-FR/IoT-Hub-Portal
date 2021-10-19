// Copyright (c) CGI France - Grand Est. All rights reserved.
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
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]

    public class ConfigsController : ControllerBase
    {
        private readonly ILogger<ConfigsController> logger;

        private readonly RegistryManager registryManager;

        public ConfigsController(ILogger<ConfigsController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
        }

        /// <summary>
        /// Checks if the specific metric (targeted/applied/success/failure) exists within the device,
        /// Returns the corresponding value if so, else returns -1 as an error code.
        /// </summary>
        /// <param name="item">Configuration item to convert into a ConfigListItem.</param>
        /// <param name="metricName">Metric to retrieve (targetedCount, appliedCount, reportedSuccessfulCount or reportedFailedCount). </param>
        /// <returns>Corresponding metric value, or -1 if it doesn't exist.</returns>
        private static long RetrieveMetricValue(Configuration item, string metricName)
        {
            if (item.SystemMetrics.Results.Keys.Contains(metricName))
                return item.SystemMetrics.Results[metricName];
            else
                return -1;
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
            List<Configuration> configList = (List<Configuration>)await this.registryManager.GetConfigurationsAsync(0);
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

                    ConfigListItem result = CreateConfigListItem(config, moduleList);
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieve a specific deployment and its modules from the IoT Hub.
        /// Converts it as a ConfigListItem.
        /// </summary>
        /// <param name="configurationID">ID of the deployment to retrieve.</param>
        /// <returns>The ConfigListItem corresponding to the given ID.</returns>
        [HttpGet("{configurationID}")]
        public async Task<ConfigListItem> Get(string configurationID)
        {
            var config = await this.registryManager.GetConfigurationAsync(configurationID);
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
                                GatewayModule newModule = this.CreateGatewayModule(config, m);
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
                                GatewayModule newModule = this.CreateGatewayModule(config, sm);
                                moduleList.Add(newModule);
                            }
                        }
                    }
                }
            }

            ConfigListItem result = CreateConfigListItem(config, moduleList);
            return result;
        }

        /// <summary>
        /// Create a ConfigListItem from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="moduleList">List of modules related to this configuration.</param>
        /// <returns>A configuration converted to a ConfigListItem.</returns>
        private static ConfigListItem CreateConfigListItem(Configuration config, List<GatewayModule> moduleList)
        {
            return new ConfigListItem
            {
                ConfigurationID = config.Id,
                Conditions = config.TargetCondition,
                MetricsTargeted = RetrieveMetricValue(config, "targetedCount"),
                MetricsApplied = RetrieveMetricValue(config, "appliedCount"),
                MetricsSuccess = RetrieveMetricValue(config, "reportedSuccessfulCount"),
                MetricsFailure = RetrieveMetricValue(config, "reportedFailedCount"),
                Priority = config.Priority,
                CreationDate = config.CreatedTimeUtc,
                Modules = moduleList
            };
        }

        /// <summary>
        /// Create a GatewayModule object from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="module">Dictionnary containing the module's name and its properties.</param>
        /// <returns>A module with all its details as a GatewayModule object.</returns>
        GatewayModule CreateGatewayModule(Configuration config, KeyValuePair<string, JToken> module)
        {
            return new GatewayModule
            {
                ModuleName = module.Key,
                Version = (string)module.Value["settings"]["image"],
                Status = (string)module.Value["status"],
                EnvironmentVariables = GetEnvironmentVariables(module),
                ModuleIdentityTwinSettings = GetModuleIdentityTwinSettings(config, module)
            };
        }

        /// <summary>
        /// Gets the module's identity twin settings from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="module">Dictionnary containing the module's name and its properties.</param>
        /// <returns>A dictionnary containing the settings and their corresponding values.</returns>
        private static Dictionary<string, string> GetModuleIdentityTwinSettings(Configuration config, KeyValuePair<string, JToken> module)
        {
            Dictionary<string, string> twinSettings = new ();

            if (config.Content.ModulesContent != null)
            {
                // Only exists if the module contains an identity twin
                if (config.Content.ModulesContent.ContainsKey(module.Key))
                {
                    // Gets the settings of the specific module based on its name (module.Key)
                    var myModuleTwin = config.Content.ModulesContent[module.Key];
                    foreach (var setting in myModuleTwin)
                    {
                        twinSettings.Add(setting.Key, setting.Value.ToString());
                    }
                }
            }

            return twinSettings;
        }

        /// <summary>
        /// Gets the module's environment variables from an Azure Module.
        /// </summary>
        /// <param name="module">Dictionnary containing the module's name and its properties.</param>
        /// <returns>A dictionnary containing the environment variables and their corresponding values.</returns>
        private static Dictionary<string, string> GetEnvironmentVariables(KeyValuePair<string, JToken> module)
        {
            Dictionary<string, string> envVariables = new ();

            // Converts the object to a JObject to access its properties more easily
            JObject moduleProperties = module.Value as JObject;

            // Only exists if the module contains environment variables
            if (moduleProperties.ContainsKey("env"))
            {
                foreach (JProperty val in moduleProperties["env"])
                {
                    var variableName = val.Name;

                    // Converts the object to a JObject to access its properties more easily
                    JObject tmp = val.Value as JObject;
                    var variableValue = tmp["value"];
                    envVariables.Add(variableName, variableValue.ToString());
                }
            }

            return envVariables;
        }
    }
}
