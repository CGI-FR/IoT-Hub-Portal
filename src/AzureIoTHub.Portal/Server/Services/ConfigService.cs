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
    using Extensions;
    using Microsoft.Azure.Devices;

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
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            return configurations.Where(c => c.Content.ModulesContent.Count > 0);
        }

        public async Task<IEnumerable<Configuration>> GetDevicesConfigurations()
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            return configurations
                .Where(c => c.Priority > 0 && c.Content.ModulesContent.Count == 0);
        }

        public Task<Configuration> GetConfigItem(string id)
        {
            return this.registryManager.GetConfigurationAsync(id);
        }

        public async Task DeleteConfiguration(string configId)
        {
            await this.registryManager.RemoveConfigurationAsync(configId);
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

        public async Task RollOutDeviceConfiguration(
            string modelId,
            Dictionary<string, object> desiredProperties,
            string configurationId,
            Dictionary<string, string> targetTags,
            int priority = 0)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            var configurationNamePrefix = configurationId.Trim().ToLowerInvariant().RemoveDiacritics();

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

            _ = await this.registryManager.AddConfigurationAsync(newConfiguration);
        }
    }
}
