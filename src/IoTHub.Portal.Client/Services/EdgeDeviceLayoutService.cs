// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;

    public class EdgeDeviceLayoutService : IEdgeDeviceLayoutService
    {
        private IoTEdgeDeviceDto sharedDevice = default!;
        private IoTEdgeModelDto sharedDeviceModel = default!;

        public event EventHandler RefreshDeviceOccurred = default!;

        public void RefreshDevice()
        {
            OnRefreshDeviceOccurred();
        }

        public IoTEdgeDeviceDto GetSharedDevice()
        {
            return this.sharedDevice;
        }

        public IoTEdgeModelDto GetSharedDeviceModel()
        {
            return this.sharedDeviceModel;
        }

        public IoTEdgeDeviceDto ResetSharedDevice(List<DeviceTagDto>? tags = null)
        {
            this.sharedDevice = new IoTEdgeDeviceDto();

            foreach (var tag in tags ?? new List<DeviceTagDto>())
            {
                _ = this.sharedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            return this.sharedDevice;
        }

        public IoTEdgeModelDto ResetSharedDeviceModel()
        {
            this.sharedDeviceModel = new IoTEdgeModelDto();

            return this.sharedDeviceModel;
        }

        public IoTEdgeDeviceDto DuplicateSharedDevice(IoTEdgeDeviceDto deviceToDuplicate)
        {
            deviceToDuplicate.DeviceId = string.Empty;
            deviceToDuplicate.DeviceName = $"{deviceToDuplicate.DeviceName} - copy";

            this.sharedDevice = deviceToDuplicate;

            return this.sharedDevice;
        }

        public IoTEdgeModelDto DuplicateSharedDeviceModel(IoTEdgeModelDto deviceModelToDuplicate)
        {
            this.sharedDeviceModel = deviceModelToDuplicate;

            return this.sharedDeviceModel;
        }

        private void OnRefreshDeviceOccurred() => RefreshDeviceOccurred?.Invoke(this, EventArgs.Empty);
    }
}
