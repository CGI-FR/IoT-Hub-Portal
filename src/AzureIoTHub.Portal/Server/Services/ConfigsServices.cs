// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Interfaces;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices;

    public class ConfigsServices : IConfigs
    {
        private readonly RegistryManager registryManager;

        public ConfigsServices(
            RegistryManager registry)
        {
            this.registryManager = registry;
        }

        public async Task<IEnumerable<Configuration>> GetAllConfigs()
        {
            return await this.registryManager.GetConfigurationsAsync(0);
        }

        public Task<Configuration> GetConfigItem(string id)
        {
            return this.registryManager.GetConfigurationAsync(id);
        }
    }
}
