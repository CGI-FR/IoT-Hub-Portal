// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
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
                UseOTAA = bool.Parse(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.UseOTAA)) ?? "True"),
                Deduplication = Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Deduplication)),
                PreferredWindow = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.PreferredWindow)), out int result) ? result : null,
                Supports32BitFCnt = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Supports32BitFCnt)), out bool boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.ABPRelaxMode)), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.KeepAliveTimeout)), out result) ? result : null,
                Downlink = bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.Downlink)), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntDownStart)), out result) ? result : null,
                FCntResetCounter = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntResetCounter)), out result) ? result : null,
                FCntUpStart = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.FCntUpStart)), out result) ? result : null,
                RX1DROffset = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RX1DROffset)), out result) ? result : null,
                RX2DataRate = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RX2DataRate)), out result) ? result : null,
                RXDelay = int.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(LoRaDeviceBase.RXDelay)), out result) ? result : null,
                CustomTags = customTags,
                DataRate = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.DataRate)),
                TxPower = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.TxPower)),
                NbRep = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.NbRep)),
                ReportedRX1DROffset = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX1DROffset)),
                ReportedRX2DataRate = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRX2DataRate)),
                ReportedRXDelay = Helpers.DeviceHelper.RetrieveReportedPropertyValue(twin, nameof(LoRaDeviceDetails.ReportedRXDelay))
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
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.UseOTAA), item.UseOTAA.ToString());

            // Update the twin properties
            if (item.UseOTAA)
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

            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.Supports32BitFCnt), item.Supports32BitFCnt);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.RXDelay), item.RXDelay);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.RX2DataRate), item.RX2DataRate);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.RX1DROffset), item.RX1DROffset);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.ABPRelaxMode), item.ABPRelaxMode);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.KeepAliveTimeout), item.KeepAliveTimeout);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.FCntDownStart), item.FCntDownStart);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.FCntResetCounter), item.FCntResetCounter);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.FCntUpStart), item.FCntUpStart);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.Deduplication), item.Deduplication);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.Downlink), item.Downlink);
            Helpers.DeviceHelper.SetDesiredProperty(twin, nameof(item.PreferredWindow), item.PreferredWindow);

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
