// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices;
    using Newtonsoft.Json.Linq;

    public static class ConfigHelper
    {
        /// <summary>
        /// Checks if the specific metric (targeted/applied/success/failure) exists within the device,
        /// Returns the corresponding value if so, else returns -1 as an error code.
        /// </summary>
        /// <param name="item">Configuration item to convert into a ConfigListItem.</param>
        /// <param name="metricName">Metric to retrieve (targetedCount, appliedCount, reportedSuccessfulCount or reportedFailedCount). </param>
        /// <returns>Corresponding metric value, or -1 if it doesn't exist.</returns>
        public static long RetrieveMetricValue(Configuration item, string metricName)
        {
            if (item.SystemMetrics.Results.Keys.Contains(metricName))
                return item.SystemMetrics.Results[metricName];
            else
                return -1;
        }

        /// <summary>
        /// Create a ConfigListItem from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="moduleList">List of modules related to this configuration.</param>
        /// <returns>A configuration converted to a ConfigListItem.</returns>
        public static ConfigListItem CreateConfigListItem(Configuration config, List<GatewayModule> moduleList)
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
        public static GatewayModule CreateGatewayModule(Configuration config, KeyValuePair<string, JToken> module)
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
