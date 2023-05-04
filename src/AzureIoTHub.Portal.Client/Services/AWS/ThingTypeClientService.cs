// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services.AWS
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.AWS;

    public class ThingTypeClientService : IThingTypeClientService
    {
        private readonly HttpClient http;

        public ThingTypeClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateThingType(ThingTypeDto thingType)
        {
            var response = await this.http.PostAsJsonAsync("api/aws/thingtypes", thingType);
            _ = response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
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
