// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;
    using Portal.Models.v10.LoRaWAN;

    public class DeviceLayoutService : IDeviceLayoutService
    {
        private DeviceDetails sharedDevice = new();
        private DeviceModel sharedDeviceModel = new();

        public event EventHandler RefreshDeviceOccurred;

        public void RefreshDevice()
        {
            OnRefreshDeviceOccurred();
        }

        public DeviceDetails GetSharedDevice()
        {
            return this.sharedDevice;
        }

        public DeviceModel GetSharedDeviceModel()
        {
            return this.sharedDeviceModel;
        }

        public DeviceDetails ResetSharedDevice(List<DeviceTag> tags = null)
        {
            this.sharedDevice = new DeviceDetails();

            foreach (var tag in tags ?? new List<DeviceTag>())
            {
                _ = this.sharedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            return this.sharedDevice;
        }

        public DeviceModel ResetSharedDeviceModel()
        {
            this.sharedDeviceModel = new DeviceModel();

            return this.sharedDeviceModel;
        }

        public DeviceDetails DuplicateSharedDevice(DeviceDetails deviceToDuplicate)
        {
            deviceToDuplicate.DeviceID = string.Empty;
            deviceToDuplicate.DeviceName = $"{deviceToDuplicate.DeviceName} - copy";

            if (deviceToDuplicate.IsLoraWan)
            {
                var loRaDeviceDetails = (LoRaDeviceDetails)deviceToDuplicate;
                loRaDeviceDetails.AppKey = string.Empty;

                this.sharedDevice = loRaDeviceDetails;
            }
            else
            {
                this.sharedDevice = deviceToDuplicate;
            }

            return this.sharedDevice;
        }

        public DeviceModel DuplicateSharedDeviceModel(DeviceModel deviceModelToDuplicate)
        {
            this.sharedDeviceModel = deviceModelToDuplicate;

            return this.sharedDeviceModel;
        }

        private void OnRefreshDeviceOccurred() => RefreshDeviceOccurred?.Invoke(this, EventArgs.Empty);
    }
}
