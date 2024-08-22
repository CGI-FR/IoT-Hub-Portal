// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    /// <summary>
    /// Device property value.
    /// </summary>
    public class DevicePropertyValue : DeviceProperty
    {
        /// <summary>
        /// The current property value.
        /// </summary>
        public string Value { get; set; } = default!;
    }
}
