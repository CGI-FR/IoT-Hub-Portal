// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;

    public interface IDeviceDetails
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The model identifier.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public Uri ImageUrl { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public List<LabelDto> Labels { get; set; }
    }
}
