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

        public LoRaDeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<DeviceTag> tags = null)
        {
            var modelId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId));

            Dictionary<string, string> customTags = new Dictionary<string, string>();
            foreach (DeviceTag tag in tags)
            {
                customTags.Add(tag.Name, Helpers.DeviceHelper.RetrieveTagValue(twin, tag.Name));
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
                AppEUI = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI)),
                AppKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppKey)),
                SensorDecoder = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.SensorDecoder)),
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
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);


            // Update the twin properties
            twin.Properties.Desired[nameof(item.AppEUI)] = item.AppEUI;
            twin.Properties.Desired[nameof(item.AppKey)] = item.AppKey;
            twin.Properties.Desired[nameof(item.SensorDecoder)] = item.SensorDecoder;

            foreach (KeyValuePair<string, string> customTag in item.CustomTags)
            {
                Helpers.DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
            }
        }
    }
}
