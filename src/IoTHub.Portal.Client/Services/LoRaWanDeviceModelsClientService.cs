// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    public class LoRaWanDeviceModelsClientService : ILoRaWanDeviceModelsClientService
    {
        private readonly HttpClient http;

        public LoRaWanDeviceModelsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<LoRaDeviceModelDto> GetDeviceModel(string deviceModelId)
        {
            return this.http.GetFromJsonAsync<LoRaDeviceModelDto>($"api/lorawan/models/{deviceModelId}")!;
        }

        public Task CreateDeviceModel(LoRaDeviceModelDto deviceModelDto)
        {
            return this.http.PostAsJsonAsync("api/lorawan/models", deviceModelDto);
        }

        public Task UpdateDeviceModel(LoRaDeviceModelDto deviceModelDto)
        {
            return this.http.PutAsJsonAsync($"api/lorawan/models/{deviceModelDto.ModelId}", deviceModelDto);
        }

        public Task SetDeviceModelCommands(string deviceModelId, IList<DeviceModelCommandDto> commands)
        {
            return this.http.PostAsJsonAsync($"api/lorawan/models/{deviceModelId}/commands", commands);
        }

        public async Task<IList<DeviceModelCommandDto>> GetDeviceModelCommands(string deviceModelId)
        {
            return await this.http.GetFromJsonAsync<List<DeviceModelCommandDto>>($"api/lorawan/models/{deviceModelId}/commands") ?? new List<DeviceModelCommandDto>();
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
