// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    public class LoRaDeviceTwinMapper : IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaDeviceTwinMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public LoRaDeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<string> tags)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var modelId = DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId));

            var customTags = new Dictionary<string, string>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    customTags.Add(tag, DeviceHelper.RetrieveTagValue(twin, tag)!);
                }
            }

            var result = new LoRaDeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                AlreadyLoggedInOnce = DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(modelId!),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                GatewayID = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.GatewayID)),
                SensorDecoder = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.SensorDecoder)),
                UseOTAA = !string.IsNullOrEmpty(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI))),
                AppEUI = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI)),
                AppKey = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppKey)),
                DevAddr = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.DevAddr)),
                AppSKey = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppSKey)),
                NwkSKey = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.NwkSKey)),
                Deduplication = Enum.TryParse<DeduplicationMode>(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.Deduplication)), out var deduplication) ? deduplication : DeduplicationMode.None,
                ClassType = Enum.TryParse<ClassType>(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.ClassType)), out var classType) ? classType : ClassType.A,
                PreferredWindow = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.PreferredWindow)), out var preferedWindow) ? preferedWindow : default,
                Supports32BitFCnt = bool.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.Supports32BitFCnt)), out var boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.ABPRelaxMode)), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.KeepAliveTimeout)), out var keepAliveTimeout) ? keepAliveTimeout : null,
                Downlink = bool.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.Downlink)), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.FCntDownStart)), out var fcntDownStart) ? fcntDownStart : null,
                FCntResetCounter = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.FCntResetCounter)), out var fcntResetCounter) ? fcntResetCounter : null,
                FCntUpStart = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.FCntUpStart)), out var fcntUpStart) ? fcntUpStart : null,
                RX1DROffset = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.RX1DROffset)), out var rx1DataOffset) ? rx1DataOffset : null,
                RX2DataRate = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.RX2DataRate)), out var rx2DataRate) ? rx2DataRate : null,
                RXDelay = int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.RXDelay)), out var rxDelay) ? rxDelay : null,
                DataRate = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.DataRate)),
                TxPower = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.TxPower)),
                NbRep = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.NbRep)),
                ReportedRX1DROffset = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX1DROffset)),
                ReportedRX2DataRate = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX2DataRate)),
                ReportedRXDelay = DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRXDelay))
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
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))!),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                SupportLoRaFeatures = bool.Parse(DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures)) ?? "false")
            };
        }

        public void UpdateTwin(Twin twin, LoRaDeviceDetails item)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            // Update the twin properties
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures), "true");

            DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            // Update OTAA settings
            DeviceHelper.SetDesiredProperty(twin, nameof(item.AppEUI), item.AppEUI);
            DeviceHelper.SetDesiredProperty(twin, nameof(item.AppKey), item.AppKey);

            // Update ABP settings
            DeviceHelper.SetDesiredProperty(twin, nameof(item.NwkSKey), item.NwkSKey);
            DeviceHelper.SetDesiredProperty(twin, nameof(item.AppSKey), item.AppSKey);
            DeviceHelper.SetDesiredProperty(twin, nameof(item.DevAddr), item.DevAddr);

            DeviceHelper.SetDesiredProperty(twin, nameof(item.GatewayID), item.GatewayID);

            if (item.Tags != null)
            {
                foreach (var customTag in item.Tags)
                {
                    DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
                }
            }
        }
    }
}
