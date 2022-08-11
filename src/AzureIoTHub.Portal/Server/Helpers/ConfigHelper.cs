// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule;
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
        /// Create a ConfigListItem from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <returns>A configuration converted to a ConfigListItem.</returns>
        public static ConfigListItem CreateConfigListItem(Configuration config)
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

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
            };
        }

        /// <summary>
        /// Create a DeviceConfig from an Azure Configuration.
        /// </summary>
        /// <param name="config">Configuration object from Azure IoT Hub.</param>
        /// <returns>A configuration converted to a DeviceConfig.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DeviceConfig CreateDeviceConfig(Configuration config)
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            // Define a regular expression for repeated words.
            var rx = new Regex(@"tags[.](?<tagName>\w*)[ ]?[=][ ]?\'(?<tagValue>[\w-]*)\'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (string.IsNullOrEmpty(config.TargetCondition))
            {
                throw new InvalidOperationException("Target condition is null.");
            }

            var matches = rx.Matches(config.TargetCondition);

            if (matches.Count == 0)
            {
                throw new InvalidOperationException("Target condition is not formed as expected.");
            }

            var result = new DeviceConfig
            {
                ConfigurationId = config.Labels.TryGetValue("configuration-id", out var id) ? id: config.Id,
                Priority = config.Priority
            };

            // Find matches.
            matches.Select(match => match.Groups).ToList()
                .ForEach(group =>
                {
                    result.Tags.Add(group["tagName"].Value, group["tagValue"].Value);
                });

            foreach (var item in config.Content.DeviceContent)
            {
                result.Properties.Add(item.Key.Replace("properties.desired.", null, StringComparison.OrdinalIgnoreCase), item.Value?.ToString());
            }

            if (result.Tags.TryGetValue("modelId", out var modelId))
            {
                result.ModelId = modelId;

                _ = result.Tags.Remove("modelId");
            }

            return result;
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
                ImageURI = module.Value["settings"]["image"]?.Value<string>(),
                Status = module.Value["status"]?.Value<string>(),
                Version = module.Value["version"]?.Value<string>(),
            };

            foreach (var item in GetEnvironmentVariables(module))
            {
                result.EnvironmentVariables.Add(
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = item.Key,
                        Value = item.Value
                    });
            }

            foreach (var item in GetModuleIdentityTwinSettings(config, module))
            {
                result.ModuleIdentityTwinSettings.Add(
                new IoTEdgeModuleTwinSetting()
                {
                    Name = item.Key,
                    Value = item.Value
                });
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
                foreach (var val in environmentVariables.Cast<JProperty>())
                {
                    envVariables.Add(val.Name, val.Value["value"].Value<string>());
                }
            }

            return envVariables;
        }

        public static Dictionary<string, IDictionary<string, object>> GenerateModulesContent(IoTEdgeModel edgeModel)
        {
            var edgeAgentPropertiesDesired = new EdgeAgentPropertiesDesired();
            var edgeHubPropertiesDesired = new EdgeHubPropertiesDesired();

            foreach (var module in edgeModel.EdgeModules)
            {
                var configModule = new ConfigModule
                {
                    Type = "docker",
                    Status = module.Status,
                    Settings = new ModuleSettings()
                    {
                        Image = module.ImageURI
                    },
                    Version = module.Version,
                    RestarPolicy = "always",
                    EnvironmentVariables = new Dictionary<string, EnvironmentVariable>()
                };

                foreach (var env in module.EnvironmentVariables)
                {
                    configModule.EnvironmentVariables.Add(env.Name, new EnvironmentVariable() { EnvValue = env.Value });
                }

                edgeAgentPropertiesDesired.Modules.Add(module.ModuleName, configModule);
            }

            return new Dictionary<string, IDictionary<string, object>>
            {
                {
                    "$edgeAgent",
                    new Dictionary<string , object>()
                    {
                        {
                            "properties.desired", edgeAgentPropertiesDesired
                        }
                    }
                },
                {
                    "$edgeHub",
                    new Dictionary<string , object>()
                    {
                        {
                            "properties.desired", edgeHubPropertiesDesired
                        }
                    }
                }
            };
        }
    }
}
