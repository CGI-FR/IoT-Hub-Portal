// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;
    using Newtonsoft.Json.Linq;

    public static class ConfigHelper
    {
        /// <summary>
        /// Checks if the specific metric (targeted/applied/success/failure) exists within the device,
        /// Returns the corresponding value if so, else returns 0.
        /// </summary>
        /// <param name="item">Configuration item to convert into a ConfigListItem.</param>
        /// <param name="metricName">Metric to retrieve (targetedCount, appliedCount, reportedSuccessfulCount or reportedFailedCount). </param>
        /// <returns>Corresponding metric value, or 0 if it doesn't exist.</returns>
        public static long RetrieveMetricValue(Configuration item, string metricName)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            if (item.SystemMetrics.Results.TryGetValue(metricName, out var result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Create a ConfigListItem from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="moduleList">List of modules related to this configuration.</param>
        /// <returns>A configuration converted to a ConfigListItem.</returns>
        public static ConfigListItem CreateConfigListItem(Configuration config, IReadOnlyCollection<IoTEdgeModule> moduleList)
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));
            ArgumentNullException.ThrowIfNull(moduleList, nameof(moduleList));

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
        public static IoTEdgeModule CreateGatewayModule(Configuration config, JProperty module)
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));
            ArgumentNullException.ThrowIfNull(module, nameof(module));

            var result = new IoTEdgeModule
            {
                ModuleName = module.Name,
                Version = module.Value["settings"]["image"]?.Value<string>(),
                Status = module.Value["status"]?.Value<string>(),
            };

            foreach (var item in GetEnvironmentVariables(module))
            {
                result.EnvironmentVariables.Add(item.Key, item.Value);
            }

            foreach (var item in GetModuleIdentityTwinSettings(config, module))
            {
                result.ModuleIdentityTwinSettings.Add(item.Key, item.Value);
            }

            return result;
        }

        /// <summary>
        /// Gets the module's identity twin settings from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <param name="module">Dictionnary containing the module's name and its properties.</param>
        /// <returns>A dictionnary containing the settings and their corresponding values.</returns>
        private static Dictionary<string, string> GetModuleIdentityTwinSettings(Configuration config, JToken module)
        {
            var twinSettings = new Dictionary<string, string>();

            if (config.Content.ModulesContent != null
                && config.Content.ModulesContent.TryGetValue(module.Path, out var modulesContent))
            {
                foreach (var setting in modulesContent)
                {
                    twinSettings.Add(setting.Key, setting.Value.ToString());
                }
            }

            return twinSettings;
        }

        /// <summary>
        /// Gets the module's environment variables from an Azure Module.
        /// </summary>
        /// <param name="module">Dictionnary containing the module's name and its properties.</param>
        /// <returns>A dictionnary containing the environment variables and their corresponding values.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static Dictionary<string, string> GetEnvironmentVariables(JProperty module)
        {
            var envVariables = new Dictionary<string, string>();

            if (module.Value is not JObject moduleProperties)
            {
                throw new InvalidOperationException($"Unable to parse {module.Name} module properties.");
            }

            // Only exists if the module contains environment variables
            if (moduleProperties.TryGetValue("env", out var environmentVariables))
            {
                foreach (JProperty val in environmentVariables)
                {
                    envVariables.Add(val.Name, val.Value["value"].Value<string>());
                }
            }

            return envVariables;
        }
    }
}
