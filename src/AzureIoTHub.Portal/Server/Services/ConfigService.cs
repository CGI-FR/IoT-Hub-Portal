// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Exceptions;
    using Extensions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Extensions;

    public class ConfigService : IConfigService
    {
        private readonly RegistryManager registryManager;

        public ConfigService(
            RegistryManager registry)
        {
            this.registryManager = registry;
        }

        public async Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations()
        {
            try
            {
                var configurations = await this.registryManager.GetConfigurationsAsync(0);
                return configurations.Where(c => c.Content.ModulesContent.Count > 0);
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException("Unable to get IOT Edge configurations", ex);
            }
        }

        public async Task<IEnumerable<Configuration>> GetDevicesConfigurations()
        {
            try
            {
                var configurations = await this.registryManager.GetConfigurationsAsync(0);

                return configurations
                    .Where(c => c.Priority > 0 && c.Content.ModulesContent.Count == 0);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException("Unable to get devices configurations", e);
            }
        }

        public async Task<Configuration> GetConfigItem(string id)
        {
            try
            {
                return await this.registryManager.GetConfigurationAsync(id);
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Unable to get the configuration for id {id}", ex);
            }
        }

        public async Task DeleteConfiguration(string configId)
        {
            try
            {
                await this.registryManager.RemoveConfigurationAsync(configId);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException($"Unable to delete the configuration for id {configId}", e);
            }
        }

        public async Task RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

#pragma warning disable CA1308 // Normalize strings to uppercase
            var configurationNamePrefix = modelId?.Trim()
                                                .ToLowerInvariant()
                                                .Replace(" ", "-", StringComparison.OrdinalIgnoreCase);
#pragma warning restore CA1308 // Normalize strings to uppercase

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await this.registryManager.RemoveConfigurationAsync(item.Id);
            }

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");

            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.TargetCondition = $"tags.modelId = '{modelId}'";
            newConfiguration.Content.DeviceContent = desiredProperties;

            _ = await this.registryManager.AddConfigurationAsync(newConfiguration);
        }

        public async Task RollOutEdgeModelConfiguration(string modelId, Dictionary<string, object> EdgeModules)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            var configurationNamePrefix = modelId?.Trim()
                                                .ToLowerInvariant()
                                                .Replace(" ", "-", StringComparison.OrdinalIgnoreCase);

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await this.registryManager.RemoveConfigurationAsync(item.Id);
            }

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");
            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.TargetCondition = $"tags.modelId = '{modelId}'";
            //newConfiguration.Content.ModuleContent = EdgeModules;
            newConfiguration.Content.ModuleContent = new Dictionary<string, object>()
            {
                {
                    "$edgeAgent", new
                    {
                        propertiesDesired = new
                        {
                            module = new Dictionary<string, object>(),
                            runtime = new
                            {
                                settings = new
                                {
                                    minDockerVersion = "v1.25"
                                },
                                type = "docker"
                            },
                            schemaVersion = "v1.1",
                            systemModules = new{}
                        }
                    }
                },
                {
                    "edgeHub",new
                    {
                        propertiesDesired = new
                        {
                                route = new {},
                                schemaVersion = "1.1",
                                storeAndForwardConfiguration = new
                                {
                                    timeToLiveSecs = 7200
                                }
                        }
                    }
                }
            };

            try
            {
                _ = await this.registryManager.AddConfigurationAsync(newConfiguration);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException("Unable to create configuration.", e);
            }
        }

        public async Task RollOutDeviceConfiguration(
            string modelId,
            Dictionary<string, object> desiredProperties,
            string configurationId,
            Dictionary<string, string> targetTags,
            int priority = 0)
        {
            IEnumerable<Configuration> configurations;

            try
            {
                configurations = await this.registryManager.GetConfigurationsAsync(0);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException("Unable to get configurations", e);
            }

            var configurationNamePrefix = configurationId.Trim().ToLowerInvariant().RemoveDiacritics();

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    await this.registryManager.RemoveConfigurationAsync(item.Id);
                }
                catch (Exception e)
                {
                    throw new InternalServerErrorException($"Unable to remove configuration {item.Id}", e);
                }
            }

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");

            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.Labels.Add("configuration-id", configurationId);

            var culture = CultureInfo.CreateSpecificCulture("en-En");
            var targetCondition = new StringBuilder();

            foreach (var item in targetTags)
            {
                _ = targetCondition.AppendFormat(culture, " and tags.{0}", item.Key);
                _ = targetCondition.AppendFormat(culture, " = '{0}'", item.Value);
            }

            newConfiguration.TargetCondition = $"tags.modelId = '{modelId}'" + targetCondition;
            newConfiguration.Content.DeviceContent = desiredProperties;
            newConfiguration.Priority = priority;

            try
            {
                _ = await this.registryManager.AddConfigurationAsync(newConfiguration);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException($"Unable to add configuration {newConfiguration.Id}", e);
            }
        }

        public async Task<int> GetFailedDeploymentsCount()
        {
            try
            {
                var configurations = await this.registryManager.GetConfigurationsAsync(0);

                var failedDeploymentsCount= configurations.Where(c => c.Content.ModulesContent.Count > 0)
                    .Sum(c => c.SystemMetrics.Results.GetValueOrDefault("reportedFailedCount", 0));

                return Convert.ToInt32(failedDeploymentsCount);
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException("Unable to get failed deployments count", ex);
            }
        }
    }
}
