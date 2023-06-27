// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDeviceConfigurationsService
    {
        Task<IEnumerable<ConfigListItemDto>> GetDeviceConfigurationListAsync();

        Task<DeviceConfigDto> GetDeviceConfigurationAsync(string configurationId);

        Task<ConfigurationMetricsDto> GetConfigurationMetricsAsync(string configurationId);

        Task CreateConfigurationAsync(DeviceConfigDto deviceConfig);

        Task UpdateConfigurationAsync(DeviceConfigDto deviceConfig);

        Task DeleteConfigurationAsync(string configurationId);
    }
}
