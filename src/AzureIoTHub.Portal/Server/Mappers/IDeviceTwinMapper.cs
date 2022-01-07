﻿// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Shared.Models.Device;
    using Microsoft.Azure.Devices.Shared;

    public interface IDeviceTwinMapper
    {
        DeviceDetails CreateDeviceDetails(Twin twin);

        DeviceListItem CreateDeviceListItem(Twin twin);

        void UpdateTwin(Twin twin, DeviceDetails item);
    }
}
