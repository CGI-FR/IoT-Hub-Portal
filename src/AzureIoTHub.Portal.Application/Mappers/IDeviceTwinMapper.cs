// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices.Shared;
    using System.Collections.Generic;

    public interface IDeviceTwinMapper<TListItem, TDevice>
        where TListItem : DeviceListItem
        where TDevice : IDeviceDetails
    {
        TDevice CreateDeviceDetails(Twin twin, IEnumerable<string> tags);

        TListItem CreateDeviceListItem(Twin twin);

        void UpdateTwin(Twin twin, TDevice item);
    }
}
