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

        public Task CreateDeviceModel(DeviceModel deviceModel)
        {
            return this.http.PostAsJsonAsync("api/models", deviceModel);
        }

        public Task UpdateDeviceModel(DeviceModel deviceModel)
        {
            return this.http.PutAsJsonAsync($"api/models/{deviceModel.ModelId}", deviceModel);
        }

        public Task DeleteDeviceModel(string deviceModelId)
        {
            return this.http.DeleteAsync($"api/models/{deviceModelId}");
        }

        public async Task<IList<DeviceProperty>> GetDeviceModelModelProperties(string deviceModelId)
        {
            return await this.http.GetFromJsonAsync<List<DeviceProperty>>($"api/models/{deviceModelId}/properties");
        }

        public Task SetDeviceModelModelProperties(string deviceModelId, IList<DeviceProperty> deviceProperties)
        {
            return this.http.PostAsJsonAsync($"api/models/{deviceModelId}/properties", deviceProperties);
        }

        public Task<string> GetAvatarUrl(string deviceModelId)
        {
            return this.http.GetStringAsync($"api/models/{deviceModelId}/avatar");
        }

        public async Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar)
        {
            var result = await this.http.PostAsync($"api/models/{deviceModelId}/avatar", avatar);

            _ = result.EnsureSuccessStatusCode();
        }
    }
}
