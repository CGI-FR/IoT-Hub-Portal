// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;

    public class LoraDeviceMethodManager : ILoraDeviceMethodManager
    {
        private readonly HttpClient httpClient;

        public LoraDeviceMethodManager(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommand command)
        {
            JsonContent commandContent = JsonContent.Create(new
            {
                rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Frame)),
                fport = command.Port
            });

            commandContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await this.httpClient.PostAsync($"api/cloudtodevicemessage/{deviceId}", commandContent);
        }
    }
}
