// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.Device
{
    using System;

    public class DeviceListItem
    {
        public string DeviceID { get; set; }

        public string DeviceName { get; set; }

        public string ImageUrl { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime StatusUpdatedTime { get; set; }

        public string AppEUI { get; set; }

        public string AppKey { get; set; }

        public string LocationCode { get; set; }
    }
}
