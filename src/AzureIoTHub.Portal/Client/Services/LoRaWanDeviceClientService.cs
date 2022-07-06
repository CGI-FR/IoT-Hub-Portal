// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10.LoRaWAN;

    public class LoRaWanDeviceClientService : ILoRaWanDeviceClientService
    {
        private readonly HttpClient http;

        public LoRaWanDeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<LoRaDeviceDetails> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<LoRaDeviceDetails>($"api/lorawan/devices/{deviceId}");
        }

        public Task CreateDevice(LoRaDeviceDetails device)
        {
            return this.http.PostAsJsonAsync("api/lorawan/devices", device);
        }

        public Task UpdateDevice(LoRaDeviceDetails device)
        {
            return this.http.PutAsJsonAsync("api/lorawan/devices", device);
        }

        public Task ExecuteCommand(string deviceId, string commandId)
        {
            return this.http.PostAsJsonAsync($"api/lorawan/devices/{deviceId}/_command/{commandId}", string.Empty);
        }
    }
}
