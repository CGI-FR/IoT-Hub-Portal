// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;
    using Portal.Shared.Security;


    public class RoleClientService : IRoleClientService
    {
        private readonly HttpClient http;
        private const string ApiUrlBase = "api/roles";

        // Use the named "api" HttpClient so auth header, version header and handlers (ProblemDetails) are applied
        public RoleClientService(IHttpClientFactory httpClientFactory)
        {
            this.http = httpClientFactory.CreateClient("api");
        }

        public Task<PaginationResult<RoleModel>> GetRoles(string continuationUri)
            => this.http.GetFromJsonAsync<PaginationResult<RoleModel>>(continuationUri)!;

        public Task DeleteRole(string roleId)
            => this.http.DeleteAsync($"api/roles/{roleId}");

        public async Task<RoleDetailsModel> GetRole(string roleId)
            => await this.http.GetFromJsonAsync<RoleDetailsModel>($"{ApiUrlBase}/{roleId}") ?? new RoleDetailsModel();

        public Task CreateRole(RoleDetailsModel role)
            => this.http.PostAsJsonAsync(ApiUrlBase, role);

        public Task<PortalPermissions[]> GetPermissions()
            => this.http.GetFromJsonAsync<PortalPermissions[]>("api/permissions")!;

        public Task UpdateRole(RoleDetailsModel role)
            => this.http.PutAsJsonAsync($"{ApiUrlBase}/{role.Id}", role);
    }
}
