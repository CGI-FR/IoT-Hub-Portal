// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;

    public class EdgeDeviceClientService : IEdgeDeviceClientService
    {
        private readonly HttpClient http;

        public EdgeDeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<IoTEdgeListItemDto>> GetDevices(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<IoTEdgeListItemDto>>(continuationUri)!;
        }

        public Task<IoTEdgeDeviceDto> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<IoTEdgeDeviceDto>($"api/edge/devices/{deviceId}")!;
        }

        public Task CreateDevice(IoTEdgeDeviceDto device)
        {
            return this.http.PostAsJsonAsync("api/edge/devices", device);
        }

        public Task UpdateDevice(IoTEdgeDeviceDto device)
        {
            return this.http.PutAsJsonAsync($"api/edge/devices/{device.DeviceId}", device);
        }

        public Task DeleteDevice(string deviceId)
        {
            return this.http.DeleteAsync($"api/edge/devices/{deviceId}");
        }

        public Task<DeviceCredentialsDto> GetEnrollmentCredentials(string deviceId)
        {
            return this.http.GetFromJsonAsync<DeviceCredentialsDto>($"api/edge/devices/{deviceId}/credentials")!;
        }

        public Task<string> GetEnrollmentScriptUrl(string deviceId, string templateName)
        {
            return this.http.GetStringAsync($"api/edge/devices/{deviceId}/enrollementScript/{templateName}")!;
        }

        public async Task<List<IoTEdgeDeviceLogDto>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModuleDto edgeModule)
        {
            var response = await this.http.PostAsJsonAsync($"api/edge/devices/{deviceId}/logs", edgeModule);

            return await response.Content.ReadFromJsonAsync<List<IoTEdgeDeviceLogDto>>() ?? new List<IoTEdgeDeviceLogDto>();
        }

        public async Task<C2DresultDto> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        {
            var response = await this.http.PostAsJsonAsync<HttpResponseMessage?>($"api/edge/devices/{deviceId}/{moduleName}/{methodName}", null);

            return await response.Content.ReadFromJsonAsync<C2DresultDto>() ?? new C2DresultDto();
        }

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return await this.http.GetFromJsonAsync<List<LabelDto>>("api/edge/devices/available-labels") ?? new List<LabelDto>();
        }
    }
}
