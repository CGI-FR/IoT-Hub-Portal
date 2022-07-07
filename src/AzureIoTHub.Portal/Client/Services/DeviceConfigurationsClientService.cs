// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
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

        public async Task<IList<ConfigListItem>> GetDeviceConfigurations()
        {
            return await this.http.GetFromJsonAsync<List<ConfigListItem>>("api/device-configurations");
        }

        public Task<DeviceConfig> GetDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<DeviceConfig>($"api/device-configurations/{deviceConfigurationId}");
        }

        public Task<ConfigurationMetrics> GetDeviceConfigurationMetrics(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<ConfigurationMetrics>($"api/device-configurations/{deviceConfigurationId}/metrics");
        }

        public Task CreateDeviceConfiguration(DeviceConfig deviceConfiguration)
        {
            return this.http.PostAsJsonAsync("api/device-configurations", deviceConfiguration);
        }

        public Task UpdateDeviceConfiguration(DeviceConfig deviceConfiguration)
        {
            return this.http.PutAsJsonAsync($"api/device-configurations/{deviceConfiguration.ConfigurationId}", deviceConfiguration);
        }

        public Task DeleteDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.DeleteAsync($"api/device-configurations/{deviceConfigurationId}");
        }
    }
}
