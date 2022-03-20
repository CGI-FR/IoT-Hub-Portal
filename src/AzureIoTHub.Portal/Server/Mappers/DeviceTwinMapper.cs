// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public class DeviceTwinMapper : IDeviceTwinMapper<DeviceListItem, DeviceDetails>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceTwinMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var modelId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId));
            var customTags = new Dictionary<string, string>();

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    customTags.Add(tag, Helpers.DeviceHelper.RetrieveTagValue(twin, tag));
                }
            }

            var result = new DeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.DeviceName)),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(modelId),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue
            };

            foreach (var item in customTags)
            {
                result.Tags.Add(item.Key, item.Value);
            }

            return result;
        }

        public DeviceListItem CreateDeviceListItem(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return new DeviceListItem
            {
                DeviceID = twin.DeviceId,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.DeviceName)),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))),
                SupportLoRaFeatures = bool.Parse(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures)) ?? "false")
            };
        }

        public void UpdateTwin(Twin twin, DeviceDetails item)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            // Update the twin properties
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            if (item.Tags == null)
            {
                return;
            }

            foreach (var customTag in item.Tags)
            {
                Helpers.DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
            }
        }
    }
}
