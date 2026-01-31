// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public interface IDevice
    {
        /// <summary>
        /// The device Identifier.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The device friendly name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The device Model Identifier.
        /// </summary>
        string DeviceModelId { get; set; }

        /// <summary>
        /// A value indicating whether the device is currently connected.
        /// </summary>
        bool IsConnected { get; set; }

        /// <summary>
        /// A value indicating whether the device is enabled on the platform.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// The device last status updated time.
        /// </summary>
        DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the last activity time.
        /// </summary>
        DateTime LastActivityTime { get; set; }

        /// <summary>
        /// The device labels.
        /// </summary>
        ICollection<Label> Labels { get; set; }

        /// <summary>
        /// The LayerId of the device.
        /// </summary>
        string? LayerId { get; set; }
    }
}
