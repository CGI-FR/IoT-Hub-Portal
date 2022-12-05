// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AzureIoTHub.Portal.Shared.Models.v10;

    /// <summary>
    /// IoT Edge list item.
    /// </summary>
    public class IoTEdgeListItem
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The number of devices connected on the IoT Edge.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// Gets the edge device labels.
        /// </summary>
        public IEnumerable<LabelDto> Labels { get; set; }
    }
}
