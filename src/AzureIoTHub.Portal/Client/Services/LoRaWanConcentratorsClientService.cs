// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10.LoRaWAN;

    public class LoRaWanConcentratorsClientService : ILoRaWanConcentratorsClientService
    {
        private readonly HttpClient http;

        public LoRaWanConcentratorsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<Concentrator>> GetConcentrators(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<Concentrator>>(continuationUri);
        }

        public Task<Concentrator> GetConcentrator(string deviceId)
        {
            return this.http.GetFromJsonAsync<Concentrator>($"api/lorawan/concentrators/{deviceId}");
        }

        public Task CreateConcentrator(Concentrator concentrator)
        {
            return this.http.PostAsJsonAsync("api/lorawan/concentrators", concentrator);
        }

        public Task UpdateConcentrator(Concentrator concentrator)
        {
            return this.http.PutAsJsonAsync("api/lorawan/concentrators", concentrator);
        }

        public Task DeleteConcentrator(string deviceId)
        {
            return this.http.DeleteAsync($"api/lorawan/concentrators/{deviceId}");
        }
    }
}
