// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public class DeviceClientService : IDeviceClientService
    {
        private readonly HttpClient http;

        public DeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<DeviceListItem>> GetDevices(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<DeviceListItem>>(continuationUri);
        }

        public Task<DeviceDetails> GetDevice(string deviceId)
        {
            return this.http.GetFromJsonAsync<DeviceDetails>($"api/devices/{deviceId}");
        }

        public Task CreateDevice(DeviceDetails device)
        {
            return this.http.PostAsJsonAsync("api/devices", device);
        }

        public Task UpdateDevice(DeviceDetails device)
        {
            return this.http.PutAsJsonAsync("api/devices", device);
        }

        public async Task<IList<DevicePropertyValue>> GetDeviceProperties(string deviceId)
        {
            return await this.http.GetFromJsonAsync<List<DevicePropertyValue>>($"api/devices/{deviceId}/properties");
        }

        public Task SetDeviceProperties(string deviceId, IList<DevicePropertyValue> deviceProperties)
        {
            return this.http.PostAsJsonAsync($"api/devices/{deviceId}/properties", deviceProperties);
        }

        public Task<EnrollmentCredentials> GetEnrollmentCredentials(string deviceId)
        {
            return this.http.GetFromJsonAsync<EnrollmentCredentials>($"api/devices/{deviceId}/credentials");
        }

        public Task DeleteDevice(string deviceId)
        {
            return this.http.DeleteAsync($"api/devices/{deviceId}");
        }
    }
}
