// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models;
    using Portal.Models.v10;
    using Portal.Models.v10.LoRaWAN;

    public class DeviceLayoutService : IDeviceLayoutService
    {
        private IDeviceDetails sharedDevice;
        private IDeviceModel sharedDeviceModel;

        public event EventHandler RefreshDeviceOccurred;

        public void RefreshDevice()
        {
            OnRefreshDeviceOccurred();
        }

        public TDevice GetSharedDevice<TDevice>()
            where TDevice : class, IDeviceDetails, new()
        {
            return this.sharedDevice as TDevice ?? new TDevice();
        }

        public TDeviceModel GetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel, new()
        {
            return this.sharedDeviceModel as TDeviceModel ?? new TDeviceModel();
        }

        public TDevice ResetSharedDevice<TDevice>(List<DeviceTag> tags = null)
            where TDevice : class, IDeviceDetails, new()
        {
            this.sharedDevice = new TDevice();

            foreach (var tag in tags ?? new List<DeviceTag>())
            {
                _ = this.sharedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            return this.sharedDevice as TDevice;
        }

        public TDeviceModel ResetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel, new()
        {
            this.sharedDeviceModel = new TDeviceModel();

            return this.sharedDeviceModel as TDeviceModel;
        }

        public TDevice DuplicateSharedDevice<TDevice>(TDevice deviceToDuplicate)
            where TDevice : class, IDeviceDetails, new()
        {
            deviceToDuplicate.DeviceID = string.Empty;
            deviceToDuplicate.DeviceName = $"{deviceToDuplicate.DeviceName} - copy";

            if (deviceToDuplicate is LoRaDeviceDetails loRaDevice)
            {
                loRaDevice.AppKey = string.Empty;
                this.sharedDevice = loRaDevice;
            }
            else
            {
                this.sharedDevice = deviceToDuplicate;
            }

            return this.sharedDevice as TDevice;
        }

        public TDeviceModel DuplicateSharedDeviceModel<TDeviceModel>(TDeviceModel deviceModelToDuplicate)
            where TDeviceModel : class, IDeviceModel, new()
        {
            this.sharedDeviceModel = deviceModelToDuplicate;

            return this.sharedDeviceModel as TDeviceModel;
        }

        private void OnRefreshDeviceOccurred() => RefreshDeviceOccurred?.Invoke(this, EventArgs.Empty);
    }
}
