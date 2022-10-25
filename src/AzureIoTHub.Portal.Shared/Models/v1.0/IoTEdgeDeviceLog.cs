// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System;

    public class IoTEdgeDeviceLog
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int LogLevel { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
