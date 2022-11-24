// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public class EdgeModuleCommandMethodManager : IEdgeModuleCommandMethodManager
    {
        private readonly HttpClient httpClient;

        public EdgeModuleCommandMethodManager(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> ExecuteEdgeModuleCommandMessage(string deviceId, EdgeModuleCommandDto command)
        {
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            using var commandContent = JsonContent.Create(command);

            commandContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await this.httpClient.PostAsync($"/api/cloudtodevicemessage/{deviceId}", commandContent);
        }
    }
}
