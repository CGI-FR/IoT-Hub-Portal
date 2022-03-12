// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceProperty
    {
        /// <summary>
        /// The property name
        /// </summary>
        [Required(ErrorMessage = "The property name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// The property display name
        /// </summary>
        [Required(ErrorMessage = "The display name is required.")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates whether the property is writable from the portal
        /// > Note: if writable, the property is set to the desired properties of the device twin
        /// >       otherwise, the property is read from the reported properties.
        /// </summary>
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public bool IsWritable { get; set; }

        /// <summary>
        /// The device property type
        /// </summary>
        [Required(ErrorMessage = "The property should define the expected type.")]
        public DevicePropertyType PropertyType { get; set; }
    }
}
