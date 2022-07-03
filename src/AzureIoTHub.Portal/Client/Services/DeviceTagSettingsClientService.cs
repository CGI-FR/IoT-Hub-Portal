// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Portal.Models.v10;

    public class DeviceTagSettingsClientService : IDeviceTagSettingsClientService
    {
        private readonly HttpClient http;

        public DeviceTagSettingsClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IList<DeviceTag>> GetDeviceTags()
        {
            return await this.http.GetFromJsonAsync<List<DeviceTag>>("api/settings/device-tags");
        }
    }
}
