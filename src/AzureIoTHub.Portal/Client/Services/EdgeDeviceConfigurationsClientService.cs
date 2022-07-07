// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public class EdgeDeviceConfigurationsClientService : IEdgeDeviceConfigurationsClientService
    {
        private readonly HttpClient http;

        public EdgeDeviceConfigurationsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<ConfigListItem>> GetDeviceConfigurations()
        {
            return await this.http.GetFromJsonAsync<List<ConfigListItem>>("api/edge/configurations");
        }

        public Task<ConfigListItem> GetDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<ConfigListItem>($"api/edge/configurations/{deviceConfigurationId}");
        }
    }
}
