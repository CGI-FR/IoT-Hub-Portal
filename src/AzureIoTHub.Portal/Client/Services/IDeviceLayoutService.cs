// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models;
    using Portal.Models.v10;

    public interface IDeviceLayoutService
    {
        event EventHandler RefreshDeviceOccurred;

        void RefreshDevice();

        TDevice GetSharedDevice<TDevice>()
            where TDevice : class, IDeviceDetails;
        TDeviceModel GetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel;
        TDevice ResetSharedDevice<TDevice>(List<DeviceTag> tags = null)
            where TDevice : class, IDeviceDetails, new();
        TDeviceModel ResetSharedDeviceModel<TDeviceModel>()
            where TDeviceModel : class, IDeviceModel, new();
        TDevice DuplicateSharedDevice<TDevice>(TDevice deviceToDuplicate)
            where TDevice : class, IDeviceDetails;
        TDeviceModel DuplicateSharedDeviceModel<TDeviceModel>(TDeviceModel deviceModelToDuplicate)
            where TDeviceModel : class, IDeviceModel;
    }
}
