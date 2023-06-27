// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.WebUtilities;
    using Portal.Models.v10;

    public class DeviceModelsClientService : IDeviceModelsClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/models";

        public DeviceModelsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<PaginationResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilterDto? deviceModelFilter = null)
        {
            var query = new Dictionary<string, string>
            {
                { nameof(DeviceModelFilterDto.SearchText), deviceModelFilter?.SearchText ?? string.Empty },
#pragma warning disable CA1305
                { nameof(DeviceModelFilterDto.PageNumber), deviceModelFilter?.PageNumber.ToString() ?? string.Empty },
                { nameof(DeviceModelFilterDto.PageSize), deviceModelFilter?.PageSize.ToString() ?? string.Empty },
#pragma warning restore CA1305
                { nameof(DeviceModelFilterDto.OrderBy), string.Join("", deviceModelFilter?.OrderBy!) ?? string.Empty }
            };

            var uri = QueryHelpers.AddQueryString(this.apiUrlBase, query);

            return await this.http.GetFromJsonAsync<PaginationResult<DeviceModelDto>>(uri) ?? new PaginationResult<DeviceModelDto>();
        }

        public Task<DeviceModelDto> GetDeviceModel(string deviceModelId)
        {
            return this.http.GetFromJsonAsync<DeviceModelDto>($"api/models/{deviceModelId}")!;
        }

        public async Task<DeviceModelDto> CreateDeviceModel(DeviceModelDto deviceModel)
        {
            var response = await this.http.PostAsJsonAsync("api/models", deviceModel);

            return await response.Content.ReadFromJsonAsync<DeviceModelDto>();
        }

        public Task UpdateDeviceModel(DeviceModelDto deviceModel)
        {
            return this.http.PutAsJsonAsync($"api/models/{deviceModel.ModelId}", deviceModel);
        }

        public Task DeleteDeviceModel(string deviceModelId)
        {
            return this.http.DeleteAsync($"api/models/{deviceModelId}");
        }

        public async Task<IList<DevicePropertyDto>> GetDeviceModelModelProperties(string deviceModelId)
        {
            return await this.http.GetFromJsonAsync<List<DevicePropertyDto>>($"api/models/{deviceModelId}/properties") ?? new List<DevicePropertyDto>();
        }

        public Task SetDeviceModelModelProperties(string deviceModelId, IList<DevicePropertyDto> deviceProperties)
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
