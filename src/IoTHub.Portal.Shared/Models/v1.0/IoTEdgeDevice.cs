// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// IoT Edge device.
    /// </summary>
    public class IoTEdgeDevice
    {
        /// <summary>
        /// The IoT Edge identifier.
        /// </summary>
        public string DeviceId { get; set; } = default!;

        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// The model identifier.
        /// </summary>
        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; } = default!;

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public Uri ImageUrl { get; set; } = default!;

        /// <summary>
        /// The IoT Edge connection state.
        /// </summary>
        public string ConnectionState { get; set; } = default!;

        /// <summary>
        /// The IoT Edge scope tag value.
        /// </summary>
        public string Scope { get; set; } = default!;

        /// <summary>
        /// The IoT Edge device status.
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// The IoT Edge runtime response.
        /// </summary>
        public string RuntimeResponse { get; set; } = default!;

        /// <summary>
        /// The number of connected devices on IoT Edge device.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The number of modules on IoT Edge device.
        /// </summary>
        public int NbModules { get; set; }

        /// <summary>
        /// The IoT Edge configuraton.
        /// </summary>
        public ConfigItem LastDeployment { get; set; }

        /// <summary>
        /// The IoT Edge modules.
        /// </summary>
        public IReadOnlyCollection<IoTEdgeModule> Modules { get; set; } = new List<IoTEdgeModule>();

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public List<LabelDto> Labels { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="IoTEdgeDevice"/> class.
        /// </summary>
        public IoTEdgeDevice()
        {
            this.IsEnabled = true;
        }
    }
}
