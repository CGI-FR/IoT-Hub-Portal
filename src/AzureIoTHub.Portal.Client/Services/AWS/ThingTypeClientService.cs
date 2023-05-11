// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.WebUtilities;

    public class ThingTypeClientService : IThingTypeClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/aws/thingtypes";


        public ThingTypeClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<PaginationResult<ThingTypeDto>> GetThingTypes(DeviceModelFilter? deviceModelFilter = null)
        {
            var query = new Dictionary<string, string>
            {
                { nameof(DeviceModelFilter.SearchText), deviceModelFilter?.SearchText ?? string.Empty },
#pragma warning disable CA1305
                { nameof(DeviceModelFilter.PageNumber), deviceModelFilter?.PageNumber.ToString() ?? string.Empty },
                { nameof(DeviceModelFilter.PageSize), deviceModelFilter?.PageSize.ToString() ?? string.Empty },
#pragma warning restore CA1305
                { nameof(DeviceModelFilter.OrderBy), string.Join("", deviceModelFilter?.OrderBy!) ?? string.Empty }
            };

            var uri = QueryHelpers.AddQueryString(this.apiUrlBase, query);
            return await this.http.GetFromJsonAsync<PaginationResult<ThingTypeDto>>(uri) ?? new PaginationResult<ThingTypeDto>();
        }

        public async Task<ThingTypeDto> GetThingType(string thingTypeId)
        {
            return await this.http.GetFromJsonAsync<ThingTypeDto>($"api/aws/thingtypes/{thingTypeId}")!;
        }


        public async Task<string> CreateThingType(ThingTypeDto thingType)
        {
            var response = await this.http.PostAsJsonAsync("api/aws/thingtypes", thingType);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task DeprecateThingType(string thingTypeId)
        {
            var result = await this.http.PutAsync($"api/aws/thingtypes/{thingTypeId}", null);
            _ = result.EnsureSuccessStatusCode();

        }

        public async Task DeleteThingType(string thingTypeId)
        {
            var result = await this.http.DeleteAsync($"api/aws/thingtypes/{thingTypeId}");
            _ = result.EnsureSuccessStatusCode();

        }
        public Task<string> GetAvatarUrl(string thingTypeId)
        {
            return this.http.GetStringAsync($"api/aws/thingtypes/{thingTypeId}/avatar");
        }

        public async Task ChangeAvatar(string thingTypeId, MultipartFormDataContent avatar)
        {
            var result = await this.http.PostAsync($"api/aws/thingtypes/{thingTypeId}/avatar", avatar);

            _ = result.EnsureSuccessStatusCode();
        }
    }
}
