// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Device property.
    /// </summary>
    public class DeviceProperty
    {
        /// <summary>
        /// The property name
        /// </summary>
        [Required(ErrorMessage = "The property name is required.")]
        [RegularExpression(@"^([\w]+\.)+[\w]+|[\w]+$", ErrorMessage = "Property name must be formed by a word or words separated by a dot")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The property display name
        /// </summary>
        [Required(ErrorMessage = "The display name is required.")]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Indicates whether the property is writable from the portal
        /// > Note: if writable, the property is set to the desired properties of the device twin
        /// >       otherwise, the property is read from the reported properties.
        /// Default is false.
        /// </summary>
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public bool IsWritable { get; set; }

        /// <summary>
        /// The property display order.
        /// </summary>
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public int Order { get; set; }

        /// <summary>
        /// The device property type
        /// </summary>
        [Required(ErrorMessage = "The property should define the expected type.")]
        public DevicePropertyType PropertyType { get; set; }
    }
}
