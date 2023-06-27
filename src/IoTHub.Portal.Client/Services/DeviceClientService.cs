// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;
    using Portal.Models.v10;

    public class DeviceClientService : IDeviceClientService
    {
        private readonly HttpClient http;

        public DeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<DeviceListItemDto>> GetDevices(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<DeviceListItemDto>>(continuationUri)!;
        }

        public Task<DeviceDetailsDto> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<DeviceDetailsDto>($"api/devices/{deviceId}")!;
        }

        public async Task<string> CreateDevice(DeviceDetailsDto device)
        {
            var response = await this.http.PostAsJsonAsync("api/devices", device);

            if (device.DeviceID != null)
            {
                return device.DeviceID;
            }

            //Retrieve Device ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedDevice = Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceDetailsDto>(responseJson);

            return updatedDevice!.DeviceID;
        }

        public Task UpdateDevice(DeviceDetailsDto device)
        {
            return this.http.PutAsJsonAsync("api/devices", device);
        }

        public async Task<IList<DevicePropertyValueDto>> GetDeviceProperties(string deviceId)
        {
            return await this.http.GetFromJsonAsync<List<DevicePropertyValueDto>>($"api/devices/{deviceId}/properties") ?? new List<DevicePropertyValueDto>();
        }

        public Task SetDeviceProperties(string deviceId, IList<DevicePropertyValueDto> deviceProperties)
        {
            return this.http.PostAsJsonAsync($"api/devices/{deviceId}/properties", deviceProperties);
        }

        public Task<DeviceCredentialsDto> GetEnrollmentCredentials(string deviceId)
        {
            return this.http.GetFromJsonAsync<DeviceCredentialsDto>($"api/devices/{deviceId}/credentials")!;
        }

        public Task DeleteDevice(string deviceId)
        {
            return this.http.DeleteAsync($"api/devices/{deviceId}");
        }

        public async Task<HttpContent> ExportDeviceList()
        {
            var response = await this.http.PostAsync($"/api/admin/devices/_export", null);
            return response.Content;
        }

        public async Task<HttpContent> ExportTemplateFile()
        {
            var response = await this.http.PostAsync($"/api/admin/devices/_template", null);
            return response.Content;
        }
        public async Task<ImportResultLineDto[]> ImportDeviceList(MultipartFormDataContent dataContent)
        {
            var result = await this.http.PostAsync($"/api/admin/devices/_import", dataContent);
            _ = result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<ImportResultLineDto[]>() ?? Array.Empty<ImportResultLineDto>();
        }

        public async Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return await this.http.GetFromJsonAsync<List<LabelDto>>($"api/devices/available-labels") ?? new List<LabelDto>();
        }
    }
}
