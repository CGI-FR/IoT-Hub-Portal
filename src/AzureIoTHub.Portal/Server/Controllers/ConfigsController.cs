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

        private static long RetrieveMetricValue(Configuration item, string metricName)
        {
            if (item.SystemMetrics.Results.Keys.Contains(metricName))
                return item.SystemMetrics.Results[metricName];
            else
                return -1;
        }

        [HttpGet]
        public async Task<IEnumerable<ConfigListItem>> Get()
        {
            List<Configuration> configList = (List<Configuration>)await this.registryManager.GetConfigurationsAsync(10);
            var results = new List<ConfigListItem>();

            foreach (Configuration config in configList)
            {
                List<GatewayModule> moduleList = new ();

                // S'il n'y a pas de module, il s'agit d'une configuration et non d'un déploiement -> L'exclure
                if (config.Content.ModulesContent != null)
                {
                    foreach (var module in config.Content.ModulesContent)
                    {
                        var newModule = new GatewayModule
                        {
                            ModuleName = module.Key
                        };
                        moduleList.Add(newModule);
                    }

                    ConfigListItem result = this.CreateConfigListItem(config, moduleList);
                    results.Add(result);
                }
            }

            return results;
        }

        [HttpGet("{configurationID}")]
        public async Task<ConfigListItem> Get(string configurationID)
        {
            var config = await this.registryManager.GetConfigurationAsync(configurationID);
            List<GatewayModule> moduleList = new ();
            if (config.Content.ModulesContent != null)
            {
                if (config.Content.ModulesContent.ContainsKey("$edgeAgent"))
                {
                    if (config.Content.ModulesContent["$edgeAgent"].ContainsKey("properties.desired"))
                    {
                        Newtonsoft.Json.Linq.JObject modObject = (Newtonsoft.Json.Linq.JObject)config.Content.ModulesContent["$edgeAgent"]["properties.desired"];
                        if (modObject.ContainsKey("modules"))
                        {
                            Newtonsoft.Json.Linq.JObject modules = (Newtonsoft.Json.Linq.JObject)modObject["modules"];
                            foreach (var m in modules)
                            {
                                GatewayModule newModule = this.CreateGatewayModule(config, m);
                                moduleList.Add(newModule);
                            }
                        }

                        if (modObject.ContainsKey("systemModules"))
                        {
                            Newtonsoft.Json.Linq.JObject systemModules = (Newtonsoft.Json.Linq.JObject)modObject["systemModules"];

                            foreach (var sm in systemModules)
                            {
                                GatewayModule newModule = this.CreateGatewayModule(config, sm);
                                moduleList.Add(newModule);
                            }
                        }
                    }
                }
            }

            ConfigListItem result = this.CreateConfigListItem(config, moduleList);
            return result;
        }

        ConfigListItem CreateConfigListItem(Configuration config, List<GatewayModule> moduleList)
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

        GatewayModule CreateGatewayModule(Configuration config, System.Collections.Generic.KeyValuePair<string, Newtonsoft.Json.Linq.JToken> module)
        {
            return new GatewayModule
            {
                ModuleName = module.Key,
                Version = (string)module.Value["settings"]["image"],
                Status = (string)module.Value["status"],
                EnvironmentVariables = this.GetEnvironmentVariables(module),
                ModuleIdentityTwinSettings = this.GetModuleIdentityTwinSettings(config, module)
            };
        }

        Dictionary<string, string> GetModuleIdentityTwinSettings(Configuration config, System.Collections.Generic.KeyValuePair<string, Newtonsoft.Json.Linq.JToken> module)
        {
            Dictionary<string, string> twinSettings = new ();

            if (config.Content.ModulesContent != null)
            {
                if (config.Content.ModulesContent.ContainsKey(module.Key))
                {
                    var myModuleTwin = config.Content.ModulesContent[module.Key];
                    foreach (var setting in myModuleTwin)
                    {
                        twinSettings.Add(setting.Key, setting.Value.ToString());
                    }
                }
            }

            return twinSettings;
        }

        Dictionary<string, string> GetEnvironmentVariables(System.Collections.Generic.KeyValuePair<string, Newtonsoft.Json.Linq.JToken> module)
        {
            Dictionary<string, string> envVariables = new ();

            Newtonsoft.Json.Linq.JObject test = (Newtonsoft.Json.Linq.JObject)module.Value;
            if (test.ContainsKey("env"))
            {
                foreach (Newtonsoft.Json.Linq.JProperty val in module.Value["env"])
                {
                    var variableName = val.Name;
                    Newtonsoft.Json.Linq.JObject tmp = (Newtonsoft.Json.Linq.JObject)val.Value;
                    var variableValue = tmp["value"];
                    envVariables.Add(variableName, variableValue.ToString());
                }
            }

            return envVariables;
        }
    }
}
