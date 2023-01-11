// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.WebUtilities;

    public class EdgeModelClientService : IEdgeModelClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/edge/models";

        public EdgeModelClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<IoTEdgeModelListItem>> GetIoTEdgeModelList(EdgeModelFilter? edgeModelFilter = null)
        {
            var query = new Dictionary<string, string>
            {
                { nameof(EdgeModelFilter.Keyword), edgeModelFilter?.Keyword }
            };

            var uri = QueryHelpers.AddQueryString(this.apiUrlBase, query);

            return await this.http.GetFromJsonAsync<List<IoTEdgeModelListItem>>(uri);
        }

        public async Task<IoTEdgeModel> GetIoTEdgeModel(string modelId)
        {
            return await this.http.GetFromJsonAsync<IoTEdgeModel>($"{this.apiUrlBase}/{modelId}");
        }

        public Task CreateIoTEdgeModel(IoTEdgeModel model)
        {
            return this.http.PostAsJsonAsync(this.apiUrlBase, model);
        }

        public Task UpdateIoTEdgeModel(IoTEdgeModel model)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, model);
        }

        public Task DeleteIoTEdgeModel(string modelId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{modelId}");
        }

        public async Task<string> GetAvatarUrl(string modelId)
        {
            return await this.http.GetStringAsync($"{this.apiUrlBase}/{modelId}/avatar");
        }

        public Task ChangeAvatar(string id, MultipartFormDataContent content)
        {
            return this.http.PostAsync($"{this.apiUrlBase}/{id}/avatar", content);
        }

        public Task DeleteAvatar(string id)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{id}/avatar");
        }
    }
}
