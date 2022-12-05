// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;

    public interface IEdgeDeviceLayoutService
    {
        event EventHandler RefreshDeviceOccurred;

        void RefreshDevice();

        IoTEdgeDevice GetSharedDevice();
        IoTEdgeModel GetSharedDeviceModel();
        IoTEdgeDevice ResetSharedDevice(List<DeviceTagDto> tags = null);
        IoTEdgeModel ResetSharedDeviceModel();
        IoTEdgeDevice DuplicateSharedDevice(IoTEdgeDevice deviceToDuplicate);
        IoTEdgeModel DuplicateSharedDeviceModel(IoTEdgeModel deviceModelToDuplicate);
    }
}
