// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Options;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Extensions.Options;

    internal class LoRaWanManagementService : ILoRaWanManagementService
    {
        private readonly HttpClient httpClient;

        public LoRaWanManagementService(HttpClient httpClient, IOptions<LoRaWANOptions> loRaWANOptions)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(loRaWANOptions?.Value.KeyManagementUrl);
            this.httpClient.DefaultRequestHeaders.Add("x-functions-key", loRaWANOptions?.Value.KeyManagementCode);
            this.httpClient.DefaultRequestHeaders.Add("api-version", "2022-03-04");
        }

        public async Task<RouterConfig?> GetRouterConfig(string loRaRegion)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            using var resourceStream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.RouterConfigFiles.{loRaRegion}.json");

            if (resourceStream == null)
                return null;

            return await JsonSerializer.DeserializeAsync<RouterConfig>(resourceStream);
        }

        public async Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommandDto commandDto)
        {
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(commandDto, nameof(commandDto));

            var body = new LoRaCloudToDeviceMessage
            {
                RawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(commandDto.Frame)),
                Fport = commandDto.Port,
                Confirmed = commandDto.Confirmed
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
