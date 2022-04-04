// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Extensions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using AzureIoTHub.Portal.Server.Extensions;
    using Newtonsoft.Json;

    public class ConcentratorTwinMapper : IConcentratorTwinMapper
    {
        public Concentrator CreateDeviceDetails(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceName)),
                LoraRegion = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.LoraRegion)),
                ClientThumbprint = DeviceHelper.RetrieveClientThumbprintPropertyValue(twin, nameof(Concentrator.ClientThumbprint)),
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                AlreadyLoggedInOnce = DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                DeviceType = DeviceHelper.RetrieveTagValue(twin, nameof(Concentrator.DeviceType))
            };
        }

        public string RetrieveClientThumbprintValue(Twin twin)
        {
            var serializedClientThumbprint = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(Concentrator.ClientThumbprint).ToCamelCase());

            try
            {
                return JsonConvert.DeserializeObject<string[]>(serializedClientThumbprint)[0];
            }

            catch (Newtonsoft.Json.JsonReaderException)
            {
                // clientThumbprint is not an array in the device twin
                return null;
            }
            catch (System.ArgumentNullException)
            {
                // clientThumbprint does not exist in the device twin
                return null;
            }
            catch (IndexOutOfRangeException)
            {
                // clientThumbprint array is empty in the device twin
                return null;
            }
        }

        public void UpdateTwin(Twin twin, Concentrator item)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);
            
            DeviceHelper.SetDesiredProperty(twin, nameof(item.ClientThumbprint).ToCamelCase(), new string[1] { item.ClientThumbprint });
            DeviceHelper.SetDesiredProperty(twin, nameof(item.RouterConfig).ToCamelCase(), item.RouterConfig);
        }
    }
}
