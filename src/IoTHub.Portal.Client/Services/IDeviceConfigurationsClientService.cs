// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Portal.Models.v10;
    using Portal.Shared.Models.v10;

    public interface IDeviceConfigurationsClientService
    {
        Task<IList<ConfigListItemDto>> GetDeviceConfigurations();

        Task<DeviceConfigDto> GetDeviceConfiguration(string deviceConfigurationId);

        Task<ConfigurationMetricsDto> GetDeviceConfigurationMetrics(string deviceConfigurationId);

        Task CreateDeviceConfiguration(DeviceConfigDto deviceConfiguration);

        Task UpdateDeviceConfiguration(DeviceConfigDto deviceConfiguration);

        Task DeleteDeviceConfiguration(string deviceConfigurationId);
    }
}
