// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class UserClientService : IUserClientService
    {
        private readonly HttpClient http;
        private const string ApiUrlBase = "api/users";

        public UserClientService(IHttpClientFactory httpClientFactory)
        {
            http = httpClientFactory.CreateClient("api");
        }

        public Task<PaginationResult<UserModel>> GetUsers(string continuationUri)
            => http.GetFromJsonAsync<PaginationResult<UserModel>>(continuationUri)!;

        public async Task<UserDetailsModel> GetUser(string id)
        {
            var response = await http.GetAsync($"{ApiUrlBase}/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // handle not found gracefully
            }
            _ = response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDetailsModel>();
        }

        public Task CreateUser(UserDetailsModel user)
            => http.PostAsJsonAsync(ApiUrlBase, user);

        public Task UpdateUser(UserDetailsModel user)
            => http.PutAsJsonAsync($"{ApiUrlBase}/{user.Id}", user);

        public Task DeleteUser(string id)
            => http.DeleteAsync($"{ApiUrlBase}/{id}");
    }
}
