// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class Device : EntityBase
    {
        /// <summary>
        /// The device friendly name.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The device Model Identifier.
        /// </summary>
        public string DeviceModelId { get; set; } = default!;

        /// <summary>
        /// A value indicating whether the device is currently connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// A value indicating whether the device is enabled on the platform.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The device last status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the last activity time.
        /// </summary>
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// The current version of the device stored n he database
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The device model
        /// </summary>
        public DeviceModel DeviceModel { get; set; } = default!;

        /// <summary>
        /// The device labels.
        /// </summary>
        public ICollection<Label> Labels { get; set; } = default!;

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public ICollection<DeviceTagValue> Tags { get; set; } = new Collection<DeviceTagValue>();

        /// <summary>
        /// The LayerId of the device.
        /// </summary>
        public string? LayerId { get; set; }
    }
}
