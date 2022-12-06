// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.Azure.Devices;

    public interface IConfigService
    {
        Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations();

        Task<IEnumerable<Configuration>> GetDevicesConfigurations();

        Task RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties);

        Task RollOutEdgeModelConfiguration(IoTEdgeModel edgeModel);

        Task RollOutDeviceConfiguration(string modelId, Dictionary<string, object> desiredProperties, string configurationId, Dictionary<string, string> targetTags, int priority = 0);

        Task<Configuration> GetConfigItem(string id);

        Task DeleteConfiguration(string configId);

        Task<int> GetFailedDeploymentsCount();

        Task<List<IoTEdgeModule>> GetConfigModuleList(string modelId);

        Task<List<EdgeModelSystemModule>> GetModelSystemModule(string modelId);

        Task<List<IoTEdgeRoute>> GetConfigRouteList(string modelId);
    }
}
