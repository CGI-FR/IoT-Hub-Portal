// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Security;

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
