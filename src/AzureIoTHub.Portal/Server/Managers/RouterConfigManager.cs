// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public class RouterConfigManager : IRouterConfigManager
    {
        private readonly HttpClient httpClient;

        public RouterConfigManager(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<RouterConfig> GetRouterConfig(string loRaRegion)
        {
            return await this.httpClient.GetFromJsonAsync<RouterConfig>($"{loRaRegion}.json");
        }
    }
}
