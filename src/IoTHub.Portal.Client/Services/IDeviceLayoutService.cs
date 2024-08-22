// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Shared.Models;
    using Portal.Shared.Models.v1._0;

    public interface IDeviceLayoutService
    {
        event EventHandler RefreshDeviceOccurred;

        void RefreshDevice();

        IDeviceDetails GetSharedDevice();
        IDeviceModel GetSharedDeviceModel();
        TDevice ResetSharedDevice<TDevice>(List<DeviceTagDto>? tags = null)
            where TDevice : class, IDeviceDetails, new();
        TDeviceModel ResetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel, new();
        TDevice DuplicateSharedDevice<TDevice>(TDevice deviceToDuplicate)
            where TDevice : IDeviceDetails;
        TDeviceModel DuplicateSharedDeviceModel<TDeviceModel>(TDeviceModel deviceModelToDuplicate)
            where TDeviceModel : IDeviceModel;
    }
}
