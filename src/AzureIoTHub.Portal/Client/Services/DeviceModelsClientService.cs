// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public class DeviceModelsClientService : IDeviceModelsClientService
    {
        private readonly HttpClient http;

        public DeviceModelsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<DeviceModel>> GetDeviceModels()
        {
            return await this.http.GetFromJsonAsync<List<DeviceModel>>("api/models");
        }

        public Task<DeviceModel> GetDeviceModel(string deviceModelId)
        {
            return this.http.GetFromJsonAsync<DeviceModel>($"api/models/{deviceModelId}");
        }

        public async Task<IList<DeviceProperty>> GetDeviceModelModelProperties(string deviceModelId)
        {
            return await this.http.GetFromJsonAsync<List<DeviceProperty>>($"api/models/{deviceModelId}/properties");
        }
    }
}
