// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Shared.Models.v1._0;

    public class EdgeDeviceLayoutService : IEdgeDeviceLayoutService
    {
        private IoTEdgeDevice sharedDevice = default!;
        private IoTEdgeModel sharedDeviceModel = default!;

        public event EventHandler RefreshDeviceOccurred = default!;

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

        public IoTEdgeDevice ResetSharedDevice(List<DeviceTagDto>? tags = null)
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
