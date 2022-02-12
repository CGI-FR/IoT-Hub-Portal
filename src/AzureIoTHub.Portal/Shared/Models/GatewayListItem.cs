// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    public class GatewayListItem
    {
        public string DeviceId { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public int NbDevices { get; set; }
    }
}
