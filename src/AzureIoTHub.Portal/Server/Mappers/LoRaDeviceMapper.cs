// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public class LoRaDeviceTwinMapper : IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaDeviceTwinMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public LoRaDeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<string> tags)
        {
            var modelId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId));

            Dictionary<string, string> customTags = new Dictionary<string, string>();
            if(tags != null)
            {
                foreach (string tag in tags)
                {
                    customTags.Add(tag, Helpers.DeviceHelper.RetrieveTagValue(twin, tag));
                }
            }

            return new LoRaDeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                AlreadyLoggedInOnce = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(modelId),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime.GetValueOrDefault(DateTime.MinValue),
                GatewayID = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.GatewayID)),
                SensorDecoder = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.SensorDecoder)),
                AppEUI = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI)),
                AppKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppKey)),
                DevAddr = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.DevAddr)),
                AppSKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppSKey)),
                NwkSKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.NwkSKey)),
                LocationCode = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.LocationCode)),
                AssetId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.AssetId)),
                IsOTTAsetting = bool.Parse(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.IsOTTAsetting)) ?? "True"),
                CustomTags = customTags
            };
        }

        public DeviceListItem CreateDeviceListItem(Twin twin)
        {
            return new DeviceListItem
            {
                DeviceID = twin.DeviceId,
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime.GetValueOrDefault(DateTime.MinValue),
                SupportLoRaFeatures = bool.Parse(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures)) ?? "false")
            };
        }

        public void UpdateTwin(Twin twin, LoRaDeviceDetails item)
        {
            // Update the twin properties
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures), "true");

            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.IsOTTAsetting), item.IsOTTAsetting.ToString());
            // Helpers.DeviceHelper.SetTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures), true.ToString());

            // Update the twin properties
            if (item.IsOTTAsetting)
            {
                Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.AppEUI), item.AppEUI);
                Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.AppKey), item.AppKey);
            }
            else
            {
                Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.NwkSKey), item.NwkSKey);
                Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.AppSKey), item.AppSKey);
                Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.DevAddr), item.DevAddr);
            }

            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.GatewayID), item.GatewayID);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.SensorDecoder), item.SensorDecoder);

            if (item.CustomTags != null)
            {
                foreach (KeyValuePair<string, string> customTag in item.CustomTags)
                {
                    Helpers.DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
                }
            }
        }
    }
}
