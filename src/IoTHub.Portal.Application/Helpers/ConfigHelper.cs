// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Helpers
{
    using Configuration = Microsoft.Azure.Devices.Configuration;

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

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = module.Name,
                Image = module.Value["settings"]?["image"]?.Value<string>(),
                ContainerCreateOptions = module.Value["settings"]?["createOptions"]?.Value<string>(),
                StartupOrder = module.Value["settings"]?["startupOrder"]?.Value<int>() ?? 0,
                Status = module.Value["status"]?.Value<string>(),
            };

            foreach (var item in GetEnvironmentVariables(module))
            {
                edgeModule.EnvironmentVariables.Add(
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = item.Key,
                        Value = item.Value
                    });
            }

            return edgeModule;
        }

        /// <summary>
        /// Retreive and return the module twin settings.
        /// </summary>
        /// <param name="modulesContent">the configuration modules content.</param>
        /// <param name="moduleName">the module name.</param>
        /// <returns>List of IoTEdgeModuleTwinSetting.</returns>
        public static List<IoTEdgeModuleTwinSetting> CreateModuleTwinSettings(IDictionary<string, IDictionary<string, object>> modulesContent, string moduleName)
        {
            var moduleTwinSettings = new List<IoTEdgeModuleTwinSetting>();

            if (modulesContent.TryGetValue(moduleName, out var twinSettings))
            {
                foreach (var desiredProperty in twinSettings)
                {
                    moduleTwinSettings.Add(new IoTEdgeModuleTwinSetting()
                    {
                        Name = desiredProperty.Key.Replace("properties.desired.", "", StringComparison.Ordinal),
                        Value = desiredProperty.Value.ToString()
                    });
                }
            }

            return moduleTwinSettings;
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
                    envVariables.Add(val.Name, val.Value["value"]!.Value<string>()!);
                }
            }

            return envVariables;
        }

        /// <summary>
        /// Create the modules content for the new configuration.
        /// </summary>
        /// <param name="edgeModel">the IoT edge model.</param>
        /// <returns>new Dictionary.</returns>
        public static Dictionary<string, IDictionary<string, object>> GenerateModulesContent(IoTEdgeModel edgeModel)
        {
            var edgeAgentPropertiesDesired = new EdgeAgentPropertiesDesired();

            if (!string.IsNullOrEmpty(edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").Image))
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeAgent.Settings.Image = edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").Image;
            }

            if (!string.IsNullOrEmpty(edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").ContainerCreateOptions))
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeAgent.Settings.CreateOptions = edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").ContainerCreateOptions;
            }

            if (edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").StartupOrder > 0)
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeAgent.Settings.StartupOrder = edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").StartupOrder;
            }

            if (!string.IsNullOrEmpty(edgeModel.SystemModules.Single(x => x.Name == "edgeHub").Image))
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeHub.Settings.Image = edgeModel.SystemModules.Single(x => x.Name == "edgeHub").Image;
            }

            var edgeHubPropertiesDesired = GenerateRoutesContent(edgeModel.EdgeRoutes);

            if (!string.IsNullOrEmpty(edgeModel.SystemModules.Single(x => x.Name == "edgeHub").ContainerCreateOptions))
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeHub.Settings.CreateOptions = edgeModel.SystemModules.Single(x => x.Name == "edgeHub").ContainerCreateOptions;
            }

            if (edgeModel.SystemModules.Single(x => x.Name == "edgeHub").StartupOrder > 0)
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeHub.Settings.StartupOrder = edgeModel.SystemModules.Single(x => x.Name == "edgeHub").StartupOrder;
            }

            foreach (var item in edgeModel.SystemModules.Single(x => x.Name == "edgeAgent").EnvironmentVariables)
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeAgent.EnvironmentVariables?.Add(item.Name, new EnvironmentVariable() { EnvValue = item.Value });
            }

            foreach (var item in edgeModel.SystemModules.Single(x => x.Name == "edgeHub").EnvironmentVariables)
            {
                edgeAgentPropertiesDesired.SystemModules.EdgeHub.EnvironmentVariables?.Add(item.Name, new EnvironmentVariable() { EnvValue = item.Value });
            }

            var modulesContent =  new Dictionary<string, IDictionary<string, object>>();

            foreach (var module in edgeModel.EdgeModules)
            {
                var configModule = new ConfigModule
                {
                    Type = "docker",
                    Status = "running",
                    Settings = new ModuleSettings()
                    {
                        Image = module.Image,
                        CreateOptions = module.ContainerCreateOptions,
                        StartupOrder = module.StartupOrder,
                    },
                    RestartPolicy = "always",
                    EnvironmentVariables = new Dictionary<string, EnvironmentVariable>()
                };

                foreach (var env in module.EnvironmentVariables)
                {
                    configModule.EnvironmentVariables.Add(env.Name, new EnvironmentVariable() { EnvValue = env.Value });
                }

                edgeAgentPropertiesDesired.Modules.Add(module.ModuleName, configModule);

                if (module.ModuleIdentityTwinSettings.Any())
                {
                    var twinSettings = new Dictionary<string, object>();

                    foreach (var setting in module.ModuleIdentityTwinSettings)
                    {
                        twinSettings.Add($"properties.desired.{setting.Name}", setting.Value);
                    }

                    modulesContent.Add(module.ModuleName, twinSettings);
                }

            }

            modulesContent.Add("$edgeAgent",
                new Dictionary<string, object>()
                    {
                        {
                            "properties.desired", edgeAgentPropertiesDesired
                        }
                    });

            modulesContent.Add("$edgeHub",
                new Dictionary<string, object>()
                {
                    {
                        "properties.desired", edgeHubPropertiesDesired
                    }
                });


            return modulesContent;
        }

        public static EdgeHubPropertiesDesired GenerateRoutesContent(List<IoTEdgeRoute> edgeRoutes)
        {
            ArgumentNullException.ThrowIfNull(edgeRoutes, nameof(edgeRoutes));

            var edgeHubPropertiesDesired = new EdgeHubPropertiesDesired();

            // Defines routes in the IoTEdgeHub module
            foreach (var route in edgeRoutes)
            {
                var routeContent = new
                {
                    route = route.Value,
                    priority = route.Priority != null ? route.Priority : 0,
                    timeToLiveSecs = route.TimeToLive != null ? route.TimeToLive : (uint)edgeHubPropertiesDesired.StoreAndForwardConfiguration.TimeToLiveSecs
                };
                edgeHubPropertiesDesired.Routes.Add(route.Name, routeContent);
            }
            return edgeHubPropertiesDesired;
        }

        public static IoTEdgeRoute CreateIoTEdgeRouteFromJProperty(JProperty route)
        {
            ArgumentNullException.ThrowIfNull(route, nameof(route));

            return new IoTEdgeRoute
            {
                Name = route.Name,
                Value = route.Value["route"]?.Value<string>(),
                Priority = route.Value["priority"]?.Value<int>(),
                TimeToLive = route.Value["timeToLiveSecs"]?.Value<uint>(),
            };
        }
    }
}
