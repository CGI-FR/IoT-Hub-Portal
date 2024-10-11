// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using ConfigurationMetrics = Shared.Models.v10.ConfigurationMetrics;

    public interface IDeviceConfigurationsService
    {
        Task<IEnumerable<ConfigListItem>> GetDeviceConfigurationListAsync();

        Task<DeviceConfig> GetDeviceConfigurationAsync(string configurationId);

        Task<ConfigurationMetrics> GetConfigurationMetricsAsync(string configurationId);

        Task CreateConfigurationAsync(DeviceConfig deviceConfig);

        Task UpdateConfigurationAsync(DeviceConfig deviceConfig);

        Task DeleteConfigurationAsync(string configurationId);
    }
}
