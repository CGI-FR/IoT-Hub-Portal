// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0;

    public interface IDeviceConfigurationsClientService
    {
        Task<IList<ConfigListItem>> GetDeviceConfigurations();

        Task<DeviceConfig> GetDeviceConfiguration(string deviceConfigurationId);

        Task<ConfigurationMetrics> GetDeviceConfigurationMetrics(string deviceConfigurationId);

        Task CreateDeviceConfiguration(DeviceConfig deviceConfiguration);

        Task UpdateDeviceConfiguration(DeviceConfig deviceConfiguration);

        Task DeleteDeviceConfiguration(string deviceConfigurationId);
    }
}
