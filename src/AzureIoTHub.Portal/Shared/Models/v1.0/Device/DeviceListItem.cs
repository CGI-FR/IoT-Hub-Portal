// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.Device
{
    using System;
    using System.Collections.Generic;

    public class DeviceListItem
    {
        /// <summary>
        /// The device Identifier.
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// The device friendly name.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public string ImageUrl { get; set; }

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
        /// The device last status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }
    }
}
