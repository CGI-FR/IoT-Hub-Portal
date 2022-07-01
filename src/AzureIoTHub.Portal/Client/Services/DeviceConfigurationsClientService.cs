// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class DeviceConfigurationsClientService : IDeviceConfigurationsClientService
    {
        private readonly HttpClient http;

        public DeviceConfigurationsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task DeleteDeviceConfiguration(string configurationId)
        {
            return this.http.DeleteAsync($"api/device-configurations/{configurationId}");
        }
    }
}
