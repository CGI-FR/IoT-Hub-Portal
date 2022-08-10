// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;

    public interface IConfigService
    {
        Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations();

        Task<IEnumerable<Configuration>> GetDevicesConfigurations();

        Task RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties);

        Task RollOutEdgeModelConfiguration(string modelId, Dictionary<string, IoTEdgeModule> EdgeModules);

        Task RollOutDeviceConfiguration(string modelId, Dictionary<string, object> desiredProperties, string configurationId, Dictionary<string, string> targetTags, int priority = 0);

        Task<Configuration> GetConfigItem(string id);

        Task DeleteConfiguration(string configId);

        Task<int> GetFailedDeploymentsCount();

        Task<List<IoTEdgeModule>> GetConfigModuleList(string modelId);
    }
}
