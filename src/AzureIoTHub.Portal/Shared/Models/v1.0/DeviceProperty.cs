// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    public class DeviceProperty
    {
        /// <summary>
        /// The property name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The property display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates whether the property is writable from the portal
        /// > Note: if writable, the property is set to the desired properties of the device twin
        /// >       otherwise, the property is read from the reported properties.
        /// </summary>
        public bool IsWritable { get; set; }

        /// <summary>
        /// The device property type
        /// </summary>
        public DevicePropertyType PropertyType { get; set; }
    }
}
