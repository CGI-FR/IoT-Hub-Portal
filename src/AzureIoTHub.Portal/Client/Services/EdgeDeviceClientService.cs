// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using Newtonsoft.Json;
    using static System.Net.Mime.MediaTypeNames;

    public class EdgeDeviceClientService : IEdgeDeviceClientService
    {
        private readonly HttpClient http;

        public EdgeDeviceClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule module)
        {
            var response = await this.http.PostAsync($"api/edge/devices/{deviceId}/logs", new StringContent(
                JsonConvert.SerializeObject(module),
                Encoding.UTF8,
                Application.Json));

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<IoTEdgeDeviceLog>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}
