// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public class ConcentratorTwinMapper : IConcentratorTwinMapper
    {
        public ConcentratorTwinMapper()
        {
        }

        public Concentrator CreateDeviceDetails(Twin twin)
        {
            return new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceName)),
                LoraRegion = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.LoraRegion)),
                ClientCertificateThumbprint = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(Concentrator.ClientCertificateThumbprint)),
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                AlreadyLoggedInOnce = DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                DeviceType = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceType))
            };
        }

        public void UpdateTwin(Twin twin, Concentrator item)
        {
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);

            twin.Properties.Desired[nameof(item.ClientCertificateThumbprint)] = item.ClientCertificateThumbprint;
            twin.Properties.Desired[nameof(item.RouterConfig)] = item.RouterConfig;
        }
    }
}
