// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;

    public interface IDeviceLayoutService
    {
        event EventHandler RefreshDeviceOccurred;

        void RefreshDevice();

        DeviceDetails GetSharedDevice();
        DeviceModel GetSharedDeviceModel();
        DeviceDetails ResetSharedDevice(List<DeviceTag> tags = null);
        DeviceModel ResetSharedDeviceModel();
        DeviceDetails DuplicateSharedDevice(DeviceDetails deviceToDuplicate);
        DeviceModel DuplicateSharedDeviceModel(DeviceModel deviceModelToDuplicate);
    }
}
