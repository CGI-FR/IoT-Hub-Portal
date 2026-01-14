// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public class PermissionsService : IPermissionsService
    {
        private readonly HttpClient http;

        public PermissionsService(IHttpClientFactory httpClientFactory)
        {
            http = httpClientFactory.CreateClient("api");
        }

        public Task<PortalPermissions[]> GetUserPermissions()
            => http.GetFromJsonAsync<PortalPermissions[]>("api/permissions/me")!;


    }
}
