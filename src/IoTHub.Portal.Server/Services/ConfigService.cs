// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Crosscutting.Extensions;
    using IoTHub.Portal.Domain.Exceptions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Extensions;
    using Newtonsoft.Json.Linq;
    using Shared.Models.v1._0;

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
            catch (RequestFailedException ex)
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
            catch (RequestFailedException e)
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
            catch (RequestFailedException ex)
            {
                throw new InternalServerErrorException($"Unable to get the configuration for id {id}", ex);
            }
        }

        public async Task<List<IoTEdgeModule>> GetConfigModuleList(string modelId)
        {
            var configList = await GetIoTEdgeConfigurations();

            var config = configList.FirstOrDefault((x) => x.Id.StartsWith(modelId, StringComparison.Ordinal));

            if (config == null)
            {
                throw new InternalServerErrorException("Config does not exist.");
            }

            var moduleList = new List<IoTEdgeModule>();

            // Details of every modules are stored within the EdgeAgent module data
            if (config.Content.ModulesContent != null
                && config.Content.ModulesContent.TryGetValue("$edgeAgent", out var edgeAgentModule)
                && edgeAgentModule.TryGetValue("properties.desired", out var edgeAgentDesiredProperties))
            {
                // Converts the object to a JObject to access its properties more easily
                if (edgeAgentDesiredProperties is not JObject modObject)
                {
                    throw new InvalidOperationException($"Could not parse properties.desired for the configuration id {config.Id}");
                }

                // Adds regular modules to the list of modules
                if (modObject.TryGetValue("modules", out var modules))
                {
                    foreach (var newModule in modules.Values<JProperty>().Select(module => ConfigHelper.CreateGatewayModule(config, module)))
                    {
                        newModule.ModuleIdentityTwinSettings = ConfigHelper.CreateModuleTwinSettings(config.Content.ModulesContent, newModule.ModuleName);
                        moduleList.Add(newModule);
                    }
                }
            }

            return moduleList;
        }

        public async Task<List<EdgeModelSystemModule>> GetModelSystemModule(string modelId)
        {
            var configList = await GetIoTEdgeConfigurations();

            var config = configList.FirstOrDefault((x) => x.Id.StartsWith(modelId, StringComparison.Ordinal));

            if (config == null)
            {
                throw new InternalServerErrorException("Config does not exist.");
            }

            var moduleList = new List<EdgeModelSystemModule>();

            // Details of every modules are stored within the EdgeAgent module data
            if (config.Content.ModulesContent != null
                && config.Content.ModulesContent.TryGetValue("$edgeAgent", out var edgeAgentModule)
                && edgeAgentModule.TryGetValue("properties.desired", out var edgeAgentDesiredProperties))
            {
                // Converts the object to a JObject to access its properties more easily
                if (edgeAgentDesiredProperties is not JObject modObject)
                {
                    throw new InvalidOperationException($"Could not parse properties.desired for the configuration id {config.Id}");
                }

                // Adds regular modules to the list of modules
                if (modObject.TryGetValue("systemModules", out var modules))
                {
                    foreach (var newModule in modules.Values<JProperty>().Select(module => ConfigHelper.CreateGatewayModule(config, module)))
                    {
                        moduleList.Add(new EdgeModelSystemModule(newModule.ModuleName)
                        {
                            ImageUri = newModule.ImageURI,
                            EnvironmentVariables = newModule.EnvironmentVariables,
                            ContainerCreateOptions = newModule.ContainerCreateOptions,
                        });
                    }
                }
            }

            return moduleList;
        }

        public async Task<List<IoTEdgeRoute>> GetConfigRouteList(string modelId)
        {
            var configList = await GetIoTEdgeConfigurations();

            var config = configList.FirstOrDefault((x) => x.Id.StartsWith(modelId, StringComparison.Ordinal));

            if (config == null)
            {
                throw new InternalServerErrorException("Config does not exist.");
            }

            var routeList = new List<IoTEdgeRoute>();

            // Details of routes are stored within the EdgeHub properties.desired
            if (config.Content.ModulesContent != null
                && config.Content.ModulesContent.TryGetValue("$edgeHub", out var edgeHubModule)
                && edgeHubModule.TryGetValue("properties.desired", out var edgeHubDesiredProperties))
            {
                //
                if (edgeHubDesiredProperties is not JObject modObject)
                {
                    throw new InvalidOperationException($"Could not parse properties.desired for the configuration id {config.Id}");
                }

                // 
                if (modObject.TryGetValue("routes", out var routes))
                {
                    foreach (var newRoute in routes.Values<JProperty>().Select(route => ConfigHelper.CreateIoTEdgeRouteFromJProperty(route)))
                    {
                        routeList.Add(newRoute);
                    }
                }
            }

            return routeList;

        }

        public async Task DeleteConfiguration(string configId)
        {
            try
            {
                await this.registryManager.RemoveConfigurationAsync(configId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to delete the configuration for id {configId}", e);
            }
        }

        public async Task<string> RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            var configurationNamePrefix = modelId?.Trim()
                                                .ToLowerInvariant()
                                                .Replace(" ", "-", StringComparison.OrdinalIgnoreCase);
#pragma warning restore CA1308 // Normalize strings to uppercase

            await DeleteDeviceModelConfigurationByConfigurationNamePrefix(configurationNamePrefix);

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");

            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.TargetCondition = $"tags.modelId = '{modelId}'";
            newConfiguration.Content.DeviceContent = desiredProperties;

            _ = await this.registryManager.AddConfigurationAsync(newConfiguration);

            return Guid.NewGuid().ToString();
        }

        public async Task DeleteDeviceModelConfigurationByConfigurationNamePrefix(string configurationNamePrefix)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await this.registryManager.RemoveConfigurationAsync(item.Id);
            }
        }

        public async Task<string> RollOutEdgeModelConfiguration(IoTEdgeModel edgeModel)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            var configurationNamePrefix = edgeModel.ModelId?.Trim()
                                                .ToLowerInvariant()
                                                .Replace(" ", "-", StringComparison.OrdinalIgnoreCase);

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");
            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.TargetCondition = $"tags.modelId = '{edgeModel.ModelId}'";
            newConfiguration.Priority = 10;

            newConfiguration.Content.ModulesContent = ConfigHelper.GenerateModulesContent(edgeModel);

            try
            {
                _ = await this.registryManager.AddConfigurationAsync(newConfiguration);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to create configuration.", e);
            }

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await this.registryManager.RemoveConfigurationAsync(item.Id);
            }

            return Guid.NewGuid().ToString();
        }

        public async Task<string> RollOutDeviceConfiguration(
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
            catch (RequestFailedException e)
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
                catch (RequestFailedException e)
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
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to add configuration {newConfiguration.Id}", e);
            }

            return Guid.NewGuid().ToString();
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
            catch (RequestFailedException ex)
            {
                throw new InternalServerErrorException("Unable to get failed deployments count", ex);
            }
        }

        public Task<IEnumerable<IoTEdgeModule>> GetPublicEdgeModules()
        {
            return Task.FromResult<IEnumerable<IoTEdgeModule>>(Array.Empty<IoTEdgeModule>());
        }
    }
}
