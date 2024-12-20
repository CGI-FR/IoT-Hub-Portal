// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{
    /// <summary>
    /// IoT Edge list item.
    /// </summary>
    public class IoTEdgeListItem
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; } = default!;

        /// <summary>
        /// The device status.
        /// </summary>
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// The number of devices connected on the IoT Edge.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public string Image { get; set; } = default!;

        /// <summary>
        /// Gets the edge device labels.
        /// </summary>
        public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();
    }
}
