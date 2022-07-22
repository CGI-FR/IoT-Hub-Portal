// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10.LoRaWAN;

    public class LoRaWanDeviceModelsClientService : ILoRaWanDeviceModelsClientService
    {
        private readonly HttpClient http;

        public LoRaWanDeviceModelsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<LoRaDeviceModel> GetDeviceModel(string deviceModelId)
        {
            return this.http.GetFromJsonAsync<LoRaDeviceModel>($"api/lorawan/models/{deviceModelId}");
        }

        public Task CreateDeviceModel(LoRaDeviceModel deviceModel)
        {
            return this.http.PostAsJsonAsync("api/lorawan/models", deviceModel);
        }

        public Task UpdateDeviceModel(LoRaDeviceModel deviceModel)
        {
            return this.http.PutAsJsonAsync($"api/lorawan/models/{deviceModel.ModelId}", deviceModel);
        }

        public Task SetDeviceModelCommands(string deviceModelId, IList<DeviceModelCommand> commands)
        {
            return this.http.PostAsJsonAsync($"api/lorawan/models/{deviceModelId}/commands", commands);
        }

        public async Task<IList<DeviceModelCommand>> GetDeviceModelCommands(string deviceModelId)
        {
            return await this.http.GetFromJsonAsync<List<DeviceModelCommand>>($"api/lorawan/models/{deviceModelId}/commands");
        }

        public Task<string> GetAvatarUrl(string deviceModelId)
        {
            return this.http.GetStringAsync($"api/lorawan/models/{deviceModelId}/avatar");
        }

        public async Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar)
        {
            var result = await this.http.PostAsync($"api/lorawan/models/{deviceModelId}/avatar", avatar);

            _ = result.EnsureSuccessStatusCode();
        }
    }
}
