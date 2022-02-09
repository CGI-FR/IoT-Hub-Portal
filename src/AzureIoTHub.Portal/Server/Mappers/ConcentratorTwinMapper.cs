// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Shared.Models.Concentrator;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using static AzureIoTHub.Portal.Server.Startup;

    public class ConcentratorTwinMapper : IConcentratorTwinMapper
    {
        private readonly ConfigHandler configHandler;
        private readonly HttpClient httpClient;

        public ConcentratorTwinMapper(ConfigHandler configHandler)
        {
            this.configHandler = configHandler;
            this.httpClient = new HttpClient();
        }

        public Concentrator CreateDeviceDetails(Twin twin)
        {
            return new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceFriendlyName = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceFriendlyName)),
                LoraRegion = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.LoraRegion)),
                ClientCertificateThumbprint = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(Concentrator.ClientCertificateThumbprint)),
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                AlreadyLoggedInOnce = DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                DeviceType = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceType))
            };
        }

        public async Task UpdateTwin(Twin twin, Concentrator item)
        {
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceFriendlyName), item.DeviceFriendlyName);
            DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);

            twin.Properties.Desired[nameof(item.ClientCertificateThumbprint)] = item.ClientCertificateThumbprint;

            var result = await this.httpClient.GetFromJsonAsync<RouterConfig>($"{this.configHandler.LoRaRegionRouterConfigUrl}/{item.LoraRegion}.json");

            twin.Properties.Desired["routerConfig"] = result;
        }
    }
}
