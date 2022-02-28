// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

            return configurations.Where(c => c.Content.ModulesContent.Any());
        }

        public async Task<IEnumerable<Configuration>> GetDevicesConfigurations()
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            return configurations.Where(c => !c.Content.ModulesContent.Any());
        }

        public Task<Configuration> GetConfigItem(string id)
        {
            return this.registryManager.GetConfigurationAsync(id);
        }

        public async Task RolloutDeviceConfiguration(string modelId, string modelName, Dictionary<string, object> desiredProperties)
        {
            var configurations = await this.registryManager.GetConfigurationsAsync(0);

            var configurationNamePrefix = modelName.Trim()
                                                .ToLowerInvariant()
                                                .Replace(" ", "-");

            foreach (var item in configurations)
            {
                if (!item.Id.StartsWith(configurationNamePrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await this.registryManager.RemoveConfigurationAsync(item.Id);
            }

            var newConfiguration = new Configuration($"{configurationNamePrefix}-{DateTime.UtcNow.Ticks}");

            newConfiguration.Labels.Add("created-by", "Azure IoT hub Portal");
            newConfiguration.TargetCondition = $"tags.modelId = '{modelId}'";
            newConfiguration.Content.DeviceContent = desiredProperties;

            await this.registryManager.AddConfigurationAsync(newConfiguration);
        }
    }
}
