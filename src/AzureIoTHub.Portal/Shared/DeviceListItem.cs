// Copyright (c) Kevin BEAUGRAND. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared
{
    using System;

    public class DeviceListItem
    {
        public string DeviceID { get; set; }

        public bool IsConnected { get; set; }

        public DateTime LastActivityDate { get; set; }
    }
}
