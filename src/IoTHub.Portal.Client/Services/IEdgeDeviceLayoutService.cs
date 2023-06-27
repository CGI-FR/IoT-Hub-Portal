// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using Portal.Models.v10;

    public interface IEdgeDeviceLayoutService
    {
        event EventHandler RefreshDeviceOccurred;

        void RefreshDevice();

        IoTEdgeDeviceDto GetSharedDevice();
        IoTEdgeModelDto GetSharedDeviceModel();
        IoTEdgeDeviceDto ResetSharedDevice(List<DeviceTagDto>? tags = null);
        IoTEdgeModelDto ResetSharedDeviceModel();
        IoTEdgeDeviceDto DuplicateSharedDevice(IoTEdgeDeviceDto deviceToDuplicate);
        IoTEdgeModelDto DuplicateSharedDeviceModel(IoTEdgeModelDto deviceModelToDuplicate);
    }
}
