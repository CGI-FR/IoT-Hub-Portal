// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public class LoraDeviceMethodManager : ILoraDeviceMethodManager
    {
        private readonly HttpClient httpClient;

        public LoraDeviceMethodManager(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommand command)
        {
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            var body = new LoRaCloudToDeviceMessage
            {
                RawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)),
                Fport = command.Port,
                Confirmed = command.Confirmed
            };

            using var commandContent = JsonContent.Create(body);

            commandContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await this.httpClient.PostAsync($"/api/cloudtodevicemessage/{deviceId}", commandContent);
        }

        public async Task<HttpResponseMessage> CheckAzureFunctionReturn(CancellationToken cancellationToken)
        {
            return await this.httpClient.GetAsync("", cancellationToken);
        }
    }
}
