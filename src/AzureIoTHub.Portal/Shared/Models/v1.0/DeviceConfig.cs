// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System.Collections.Generic;

    public class DeviceConfig : ConfigListItem
    {
        public DeviceModel model { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public Dictionary<string, object> Properties { get; set; }

        public DeviceConfig()
        {
            Tags = new Dictionary<string, string>();
            Properties = new Dictionary<string, object>();
        }
    }
}
