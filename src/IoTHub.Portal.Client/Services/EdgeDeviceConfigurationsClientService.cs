// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public class EdgeDeviceConfigurationsClientService : IEdgeDeviceConfigurationsClientService
    {
        private readonly HttpClient http;

        public EdgeDeviceConfigurationsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<ConfigListItem>> GetDeviceConfigurations()
        {
            return await this.http.GetFromJsonAsync<List<ConfigListItem>>("api/edge/configurations") ?? new List<ConfigListItem>();
        }

        public Task<ConfigListItem> GetDeviceConfiguration(string deviceConfigurationId)
        {
            return this.http.GetFromJsonAsync<ConfigListItem>($"api/edge/configurations/{deviceConfigurationId}")!;
        }
    }
}
