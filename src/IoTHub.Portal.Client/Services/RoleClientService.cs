// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;


    public class RoleClientService : IRoleClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/roles";
        public RoleClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<RoleModel>> GetRoles(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<RoleModel>>(continuationUri)!;
        }

        public Task DeleteRole(string roleId)
        {
            return this.http.DeleteAsync($"api/roles/{roleId}");
        }

        public async Task<RoleDetailsModel> GetRole(string roleId)
        {
            return await this.http.GetFromJsonAsync<RoleDetailsModel>($"{this.apiUrlBase}/{roleId}") ?? new RoleDetailsModel();
        }

        public Task CreateRole(RoleDetailsModel role)
        {
            return this.http.PostAsJsonAsync(this.apiUrlBase, role);
        }

        public Task UpdateRole(string roleId, RoleDetailsModel role)
        {
            return this.http.PutAsJsonAsync($"{this.apiUrlBase}/{roleId}", role);
        }
    }
}
