// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Shared.Models;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    public class DeviceLayoutService : IDeviceLayoutService
    {
        private IDeviceDetails sharedDevice = default!;
        private IDeviceModel sharedDeviceModel = default!;

        public event EventHandler RefreshDeviceOccurred = default!;

        public void RefreshDevice()
        {
            OnRefreshDeviceOccurred();
        }

        public IDeviceDetails GetSharedDevice()
        {
            return this.sharedDevice;
        }

        public IDeviceModel GetSharedDeviceModel()
        {
            return this.sharedDeviceModel;
        }

        public TDevice ResetSharedDevice<TDevice>(List<DeviceTagDto>? tags = null)
            where TDevice : class, IDeviceDetails, new()
        {
            this.sharedDevice = new TDevice();

            foreach (var tag in tags ?? new List<DeviceTagDto>())
            {
                _ = this.sharedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            return (TDevice)this.sharedDevice;
        }

        public TDeviceModel ResetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel, new()
        {
            this.sharedDeviceModel = new TDeviceModel();

            return (TDeviceModel)this.sharedDeviceModel;
        }

        public TDevice DuplicateSharedDevice<TDevice>(TDevice deviceToDuplicate)
            where TDevice : IDeviceDetails
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

            return (TDevice)this.sharedDevice;
        }

        public TDeviceModel DuplicateSharedDeviceModel<TDeviceModel>(TDeviceModel deviceModelToDuplicate)
            where TDeviceModel : IDeviceModel
        {
            this.sharedDeviceModel = deviceModelToDuplicate;

            return (TDeviceModel)this.sharedDeviceModel;
        }

        private void OnRefreshDeviceOccurred() => RefreshDeviceOccurred?.Invoke(this, EventArgs.Empty);
    }
}
