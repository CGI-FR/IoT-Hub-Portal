// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;

    public class EdgeDeviceClientService : IEdgeDeviceClientService
    {
        private readonly HttpClient http;

        public EdgeDeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<IoTEdgeListItem>> GetDevices(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<IoTEdgeListItem>>(continuationUri);
        }

        public Task<IoTEdgeDevice> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<IoTEdgeDevice>($"api/edge/devices/{deviceId}");
        }

        public Task CreateDevice(IoTEdgeDevice device)
        {
            return this.http.PostAsJsonAsync("api/edge/devices", device);
        }

        public Task UpdateDevice(IoTEdgeDevice device)
        {
            return this.http.PutAsJsonAsync($"api/edge/devices/{device.DeviceId}", device);
        }

        public Task DeleteDevice(string deviceId)
        {
            return this.http.DeleteAsync($"api/edge/devices/{deviceId}");
        }

        public Task<EnrollmentCredentials> GetEnrollmentCredentials(string deviceId)
        {
            return this.http.GetFromJsonAsync<EnrollmentCredentials>($"api/edge/devices/{deviceId}/credentials");
        }

        public async Task<List<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        {
            var response = await this.http.PostAsJsonAsync($"api/edge/devices/{deviceId}/logs", edgeModule);

            return await response.Content.ReadFromJsonAsync<List<IoTEdgeDeviceLog>>();
        }

        public async Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        {
            var response = await this.http.PostAsJsonAsync<HttpResponseMessage>($"api/edge/devices/{deviceId}/{moduleName}/{methodName}", null);

            return await response.Content.ReadFromJsonAsync<C2Dresult>();
        }

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return await this.http.GetFromJsonAsync<List<LabelDto>>("api/edge/devices/available-labels");
        }
    }
}
