// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using Portal.Models.v10.LoRaWAN;

    public class LoRaWanConcentratorClientService : ILoRaWanConcentratorClientService
    {
        private readonly HttpClient http;

        public LoRaWanConcentratorClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<ConcentratorDto>> GetConcentrators(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<ConcentratorDto>>(continuationUri);
        }

        public Task<ConcentratorDto> GetConcentrator(string deviceId)
        {
            return this.http.GetFromJsonAsync<ConcentratorDto>($"api/lorawan/concentrators/{deviceId}");
        }

        public Task CreateConcentrator(ConcentratorDto concentrator)
        {
            return this.http.PostAsJsonAsync("api/lorawan/concentrators", concentrator);
        }

        public Task UpdateConcentrator(ConcentratorDto concentrator)
        {
            return this.http.PutAsJsonAsync("api/lorawan/concentrators", concentrator);
        }

        public Task DeleteConcentrator(string deviceId)
        {
            return this.http.DeleteAsync($"api/lorawan/concentrators/{deviceId}");
        }

        public Task<IEnumerable<FrequencyPlan>> GetFrequencyPlans()
        {
            return this.http.GetFromJsonAsync<IEnumerable<FrequencyPlan>>("api/lorawan/freqencyplans");
        }
    }
}
