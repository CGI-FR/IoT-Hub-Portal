// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using Microsoft.Azure.Devices.Shared;

    public interface IDeviceTwinMapper<TListItem, TDevice>
        where TListItem: DeviceListItem
        where TDevice: DeviceDetails
    {
        TDevice CreateDeviceDetails(Twin twin);

        TListItem CreateDeviceListItem(Twin twin);

        void UpdateTwin(Twin twin, TDevice item);
    }
}
