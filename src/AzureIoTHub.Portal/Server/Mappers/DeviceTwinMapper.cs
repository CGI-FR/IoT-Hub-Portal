// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.Device;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    internal class DeviceTwinMapper : IDeviceTwinMapper
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceModelCommandsManager deviceModelCommandsManager;
        private readonly ITableClientFactory tableClientFactory;

        public DeviceTwinMapper(IDeviceModelImageManager deviceModelImageManager, IDeviceModelCommandsManager deviceModelCommandsManager, ITableClientFactory tableClientFactory)
        {
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceModelCommandsManager = deviceModelCommandsManager;
            this.tableClientFactory = tableClientFactory;
        }

        public DeviceDetails CreateDeviceDetails(Twin twin)
        {
            var modelId = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId));
            // var modelName = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelName));
            return new DeviceDetails
            {
                DeviceID = twin.DeviceId,
                ModelId = modelId,
                // ModelName = modelName,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(modelId),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                LastActivityDate = twin.LastActivityTime.GetValueOrDefault(DateTime.MinValue),
                AppEUI = Helpers.DeviceHelper.RetrievePropertyValue(twin, nameof(DeviceDetails.AppEUI)),
                AppKey = Helpers.DeviceHelper.RetrievePropertyValue(twin, nameof(DeviceDetails.AppKey)),
                LocationCode = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.LocationCode)),
                AssetID = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.AssetID)),
                SensorDecoder = Helpers.DeviceHelper.RetrievePropertyValue(twin, nameof(DeviceDetails.SensorDecoder)),
                DeviceType = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.DeviceType)),
                Commands = this.deviceModelCommandsManager.RetrieveCommands(modelId)
            };
        }

        public DeviceListItem CreateDeviceListItem(Twin twin)
        {
            return new DeviceListItem
            {
                DeviceID = twin.DeviceId,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.ModelId))),
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                LocationCode = Helpers.DeviceHelper.RetrieveTagValue(twin, nameof(DeviceDetails.LocationCode)),
                LastActivityDate = twin.LastActivityTime.GetValueOrDefault(DateTime.MinValue)
            };
        }

        public void UpdateTwin(Twin twin, DeviceDetails item)
        {
            // Update the twin properties
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.LocationCode), item.LocationCode);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.AssetID), item.AssetID);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.LocationCode), item.LocationCode);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);
            // Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelName), item.ModelName);

            // Update the twin properties
            twin.Properties.Desired[nameof(item.AppEUI)] = item.AppEUI;
            twin.Properties.Desired[nameof(item.AppKey)] = item.AppKey;
            twin.Properties.Desired[nameof(item.SensorDecoder)] = item.SensorDecoder;
        }
    }
}
