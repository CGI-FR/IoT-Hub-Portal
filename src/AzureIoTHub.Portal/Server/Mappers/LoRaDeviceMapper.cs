// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
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
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var modelId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId));

            var customTags = new Dictionary<string, string>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    customTags.Add(tag, Helpers.DeviceHelper.RetrieveTagValue(twin, tag));
                }
            }

            var result = new LoRaDeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                AlreadyLoggedInOnce = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(modelId),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                GatewayID = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.GatewayID)),
                SensorDecoder = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.SensorDecoder)),
                UseOTAA = !string.IsNullOrEmpty(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI))),
                AppEUI = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppEUI)),
                AppKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppKey)),
                DevAddr = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.DevAddr)),
                AppSKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.AppSKey)),
                NwkSKey = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceDetails.NwkSKey)),
                Deduplication = Enum.TryParse<DeduplicationMode>(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Deduplication)), out var deduplication) ? deduplication : DeduplicationMode.None,
                PreferredWindow = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.PreferredWindow)), out var preferedWindow) ? preferedWindow : null,
                Supports32BitFCnt = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Supports32BitFCnt)), out var boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.ABPRelaxMode)), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.KeepAliveTimeout)), out var keepAliveTimeout) ? keepAliveTimeout : null,
                Downlink = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Downlink)), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntDownStart)), out var fcntDownStart) ? fcntDownStart : null,
                FCntResetCounter = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntResetCounter)), out var fcntResetCounter) ? fcntResetCounter : null,
                FCntUpStart = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntUpStart)), out var fcntUpStart) ? fcntUpStart : null,
                RX1DROffset = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RX1DROffset)), out var rx1DataOffset) ? rx1DataOffset : null,
                RX2DataRate = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RX2DataRate)), out var rx2DataRate) ? rx2DataRate : null,
                RXDelay = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RXDelay)), out var rxDelay) ? rxDelay : null,
                DataRate = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.DataRate)),
                TxPower = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.TxPower)),
                NbRep = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.NbRep)),
                ReportedRX1DROffset = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX1DROffset)),
                ReportedRX2DataRate = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX2DataRate)),
                ReportedRXDelay = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRXDelay))
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
                DeviceName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                StatusUpdatedTime = twin.StatusUpdatedTime ?? DateTime.MinValue,
                SupportLoRaFeatures = bool.Parse(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures)) ?? "false")
            };
        }

        public void UpdateTwin(Twin twin, LoRaDeviceDetails item)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            // Update the twin properties
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(DeviceListItem.SupportLoRaFeatures), "true");

            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            // Update OTAA settings
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.AppKey), item.AppKey);

            // Update ABP settings
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.NwkSKey), item.NwkSKey);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.AppSKey), item.AppSKey);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.DevAddr), item.DevAddr);

            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.GatewayID), item.GatewayID);

            if (item.Tags != null)
            {
                foreach (var customTag in item.Tags)
                {
                    Helpers.DeviceHelper.SetTagValue(twin, customTag.Key, customTag.Value);
                }
            }
        }
    }
}
