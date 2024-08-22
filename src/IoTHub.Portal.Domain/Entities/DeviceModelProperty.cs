// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Domain.Base;
    using Portal.Shared.Models;

    public class DeviceModelProperty : EntityBase
    {
        /// <summary>
        /// The property name
        /// </summary>
        [Required]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The property display name
        /// </summary>
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Indicates whether the property is writable from the portal
        /// > Note: if writable, the property is set to the desired properties of the device twin
        /// >       otherwise, the property is read from the reported properties.
        /// </summary>
        [Required]
        public bool IsWritable { get; set; }

        /// <summary>
        /// The property display order.
        /// </summary>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// The device property type
        /// </summary>
        [Required]
        public DevicePropertyType PropertyType { get; set; }

        /// <summary>
        /// The model identifier.
        /// </summary>
        [Required]
        public string ModelId { get; set; } = default!;
    }
}
