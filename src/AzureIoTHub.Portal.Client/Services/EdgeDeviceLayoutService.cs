// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;

    public class EdgeDeviceLayoutService : IEdgeDeviceLayoutService
    {
        private IoTEdgeDevice sharedDevice;
        private IoTEdgeModel sharedDeviceModel;

        public event EventHandler RefreshDeviceOccurred;

        public void RefreshDevice()
        {
            OnRefreshDeviceOccurred();
        }

        public IoTEdgeDevice GetSharedDevice()
        {
            return this.sharedDevice;
        }

        public IoTEdgeModel GetSharedDeviceModel()
        {
            return this.sharedDeviceModel;
        }

        public IoTEdgeDevice ResetSharedDevice(List<DeviceTagDto> tags = null)
        {
            this.sharedDevice = new IoTEdgeDevice();

            foreach (var tag in tags ?? new List<DeviceTagDto>())
            {
                _ = this.sharedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            return this.sharedDevice;
        }

        public IoTEdgeModel ResetSharedDeviceModel()
        {
            this.sharedDeviceModel = new IoTEdgeModel();

            return this.sharedDeviceModel;
        }

        public IoTEdgeDevice DuplicateSharedDevice(IoTEdgeDevice deviceToDuplicate)
        {
            deviceToDuplicate.DeviceId = string.Empty;
            deviceToDuplicate.DeviceName = $"{deviceToDuplicate.DeviceName} - copy";

            this.sharedDevice = deviceToDuplicate;

            return this.sharedDevice;
        }

        public IoTEdgeModel DuplicateSharedDeviceModel(IoTEdgeModel deviceModelToDuplicate)
        {
            this.sharedDeviceModel = deviceModelToDuplicate;

            return this.sharedDeviceModel;
        }

        private void OnRefreshDeviceOccurred() => RefreshDeviceOccurred?.Invoke(this, EventArgs.Empty);
    }
}
