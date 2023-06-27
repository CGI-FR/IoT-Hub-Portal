// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using Portal.Models.v10.LoRaWAN;

    public class LoRaWanDeviceClientService : ILoRaWanDeviceClientService
    {
        private readonly HttpClient http;

        public LoRaWanDeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<LoRaDeviceDetailsDto> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<LoRaDeviceDetailsDto>($"api/lorawan/devices/{deviceId}")!;
        }

        public Task CreateDevice(LoRaDeviceDetailsDto device)
        {
            return this.http.PostAsJsonAsync("api/lorawan/devices", device);
        }

        public Task UpdateDevice(LoRaDeviceDetailsDto device)
        {
            return this.http.PutAsJsonAsync("api/lorawan/devices", device);
        }

        public Task DeleteDevice(string deviceId)
        {
            return this.http.DeleteAsync($"api/lorawan/devices/{deviceId}");
        }

        public Task ExecuteCommand(string deviceId, string commandId)
        {
            return this.http.PostAsJsonAsync($"api/lorawan/devices/{deviceId}/_command/{commandId}", string.Empty);
        }

        public Task<LoRaGatewayIDListDto> GetGatewayIdList()
        {
            return this.http.GetFromJsonAsync<LoRaGatewayIDListDto>($"api/lorawan/devices/gateways")!;
        }

        public async Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            return await this.http.GetFromJsonAsync<List<LoRaDeviceTelemetryDto>>($"api/lorawan/devices/{deviceId}/telemetry") ?? new List<LoRaDeviceTelemetryDto>();
        }
    }
}
