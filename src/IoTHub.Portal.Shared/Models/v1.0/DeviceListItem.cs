// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{
    /// <summary>
    /// Device list item.
    /// </summary>
    public class DeviceListItem
    {
        /// <summary>
        /// The device Identifier.
        /// </summary>
        public string DeviceID { get; set; } = default!;

        /// <summary>
        /// The device friendly name.
        /// </summary>
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// The device Model Identifier.
        /// </summary>
        public string DeviceModelId { get; set; } = default!;

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public string Image { get; set; } = default!;

        /// <summary>
        /// A value indicating whether the device is currently connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// A value indicating whether the device is enabled on the platform.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// A value indicating whether the LoRa features is supported on this model.
        /// </summary>
        public bool SupportLoRaFeatures { get; set; }

        /// <summary>
        /// A value indicating whether the device has telemetry.
        /// </summary>
        public bool HasLoRaTelemetry { get; set; }

        /// <summary>
        /// The device last status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the last activity time.
        /// </summary>
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// The device labels.
        /// </summary>
        public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();

        /// <summary>
        /// The LayerId of the device.
        /// </summary>
        public string? LayerId { get; set; }
    }
}
