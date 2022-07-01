// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public class DeviceConfigurationsClientService : IDeviceConfigurationsClientService
    {
        private readonly HttpClient http;

        public DeviceConfigurationsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<ConfigListItem>> GetDeviceConfigurations()
        {
            var configurations = await this.http.GetFromJsonAsync<List<ConfigListItem>>("api/device-configurations");
            return configurations;
        }

        public Task DeleteDeviceConfiguration(string configurationId)
        {
            return this.http.DeleteAsync($"api/device-configurations/{configurationId}");
        }
    }
}
