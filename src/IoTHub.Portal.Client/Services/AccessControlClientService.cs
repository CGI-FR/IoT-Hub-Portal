// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class AccessControlClientService : IAccessControlClientService
    {
        private readonly HttpClient http;
        private const string ApiUrlBase = "api/access-controls";

        public AccessControlClientService(IHttpClientFactory httpClientFactory)
        {
            http = httpClientFactory.CreateClient("api");
        }

        public Task<PaginationResult<AccessControlModel>> GetAccessControls(string continuationUri)
            => http.GetFromJsonAsync<PaginationResult<AccessControlModel>>(continuationUri)!;

        public async Task<AccessControlModel> Create(AccessControlModel model)
        {
            var response = await http.PostAsJsonAsync(ApiUrlBase, model);
            _ = response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccessControlModel>() ?? new AccessControlModel();
        }

        public Task Delete(string id) => http.DeleteAsync($"{ApiUrlBase}/{id}");
    }
}
