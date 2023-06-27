// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;
    using Portal.Shared.Models.v10;

    public class DeviceConfigurationsClientService : IDeviceConfigurationsClientService
    {
        private readonly HttpClient http;

        public DeviceConfigurationsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<ConfigListItemDto>> GetDeviceConfigurations()
        {
            return await this.http.GetFromJsonAsync<List<ConfigListItemDto>>("api/device-configurations") ?? new List<ConfigListItemDto>();
        }

        public Task<DeviceConfigDto> GetDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<DeviceConfigDto>($"api/device-configurations/{deviceConfigurationId}")!;
        }

        public Task<ConfigurationMetricsDto> GetDeviceConfigurationMetrics(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<ConfigurationMetricsDto>($"api/device-configurations/{deviceConfigurationId}/metrics")!;
        }

        public Task CreateDeviceConfiguration(DeviceConfigDto deviceConfiguration)
        {
            return this.http.PostAsJsonAsync("api/device-configurations", deviceConfiguration);
        }

        public Task UpdateDeviceConfiguration(DeviceConfigDto deviceConfiguration)
        {
            return this.http.PutAsJsonAsync($"api/device-configurations/{deviceConfiguration.ConfigurationId}", deviceConfiguration);
        }

        public Task DeleteDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.DeleteAsync($"api/device-configurations/{deviceConfigurationId}");
        }
    }
}
