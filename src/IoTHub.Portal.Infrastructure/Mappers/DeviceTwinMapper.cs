// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public class DeviceTwinMapper : IDeviceTwinMapper<DeviceListItem, DeviceDetails>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceTwinMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNull(twin);

            var modelId = DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId));
            var customTags = new Dictionary<string, string>();

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    customTags.Add(tag, DeviceHelper.RetrieveTagValue(twin, tag)!);
                }
            }

            var result = new DeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.DeviceName)),
                Image = this.deviceModelImageManager.GetDeviceModelImageAsync(modelId!).Result,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                LastActivityTime = twin.LastActivityTime ?? DateTime.MinValue,
                LayerId = DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.LayerId))
            };

            foreach (var item in customTags)
            {
                result.Tags.Add(item.Key, item.Value);
            }

            return result;
        }

        public DeviceListItem CreateDeviceListItem(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin);

            return new DeviceListItem
            {
                DeviceID = twin.DeviceId,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                LastActivityTime = twin.LastActivityTime ?? DateTime.MinValue,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.DeviceName)),
                Image = this.deviceModelImageManager.GetDeviceModelImageAsync(DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))!).Result,
                SupportLoRaFeatures = bool.Parse(DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures)) ?? "false"),
                LayerId = DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.LayerId))
            };
        }

        public void UpdateTwin(Twin twin, DeviceDetails item)
        {
            ArgumentNullException.ThrowIfNull(twin);
            ArgumentNullException.ThrowIfNull(item);

            // Update the twin properties
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);
            DeviceHelper.SetTagValue(twin, nameof(item.LayerId), item.LayerId ?? string.Empty);

            if (item.Tags == null)
            {
                return;
            }

            foreach (var customTag in item.Tags)
            {
                DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
            }
        }
    }
}
