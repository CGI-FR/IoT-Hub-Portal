// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Azure.Devices;

    public interface IConfigService
    {
        Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations();

        Task<IEnumerable<Configuration>> GetDevicesConfigurations();

        Task<string> RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties);

        Task DeleteDeviceModelConfigurationByConfigurationNamePrefix(string configurationNamePrefix);

        Task<string> RollOutEdgeModelConfiguration(IoTEdgeModelDto edgeModel);

        Task<string> RollOutDeviceConfiguration(string modelId, Dictionary<string, object> desiredProperties, string configurationId, Dictionary<string, string> targetTags, int priority = 0);

        Task<Configuration> GetConfigItem(string id);

        Task DeleteConfiguration(string configId);

        Task<int> GetFailedDeploymentsCount();

        Task<List<IoTEdgeModuleDto>> GetConfigModuleList(string modelId);

        Task<List<EdgeModelSystemModuleDto>> GetModelSystemModule(string modelId);

        Task<List<IoTEdgeRouteDto>> GetConfigRouteList(string modelId);

        Task<IEnumerable<IoTEdgeModuleDto>> GetPublicEdgeModules();
    }
}
