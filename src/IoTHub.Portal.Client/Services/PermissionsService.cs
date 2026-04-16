// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Components.Authorization;

    public class PermissionsService : IPermissionsService
    {
        private readonly HttpClient http;
        private readonly AuthenticationStateProvider authStateProvider;
        private PortalPermissions[]? cachedPermissions;
        private string? cachedUserId;

        public PermissionsService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
        {
            this.http = httpClientFactory.CreateClient("api");
            this.authStateProvider = authStateProvider;
        }

        public async Task<PortalPermissions[]> GetUserPermissions()
        {
            var authState = await this.authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var currentUserId = user.Identity?.Name;

            if (this.cachedPermissions != null && this.cachedUserId == currentUserId)
            {
                return this.cachedPermissions;
            }

            this.cachedPermissions = await this.http.GetFromJsonAsync<PortalPermissions[]>("api/permissions/me")!;
            this.cachedUserId = currentUserId;

            return this.cachedPermissions;
        }
    }
}
