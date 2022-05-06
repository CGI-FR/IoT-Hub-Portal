// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
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
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The IoT Edge connection state.
        /// </summary>
        public string ConnectionState { get; set; }

        /// <summary>
        /// The IoT Edge scope tag value.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// The IoT Edge device type.
        /// </summary>
        [Required(ErrorMessage = "The device type is required.")]
        public string Type { get; set; }

        /// <summary>
        /// The IoT Edge device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The IoT Edge runtime response.
        /// </summary>
        public string RuntimeResponse { get; set; }

        /// <summary>
        /// The number of connected devices on IoT Edge device.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The number of modules on IoT Edge device.
        /// </summary>
        public int NbModules { get; set; }

        /// <summary>
        /// The IoT Edge environment tag value.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// The IoT Edge configuraton.
        /// </summary>
        public ConfigItem LastDeployment { get; set; }

        /// <summary>
        /// The IoT Edge modules.
        /// </summary>
        public IReadOnlyCollection<IoTEdgeModule> Modules { get; set; } = new List<IoTEdgeModule>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IoTEdgeDevice"/> class.
        /// </summary>
        public IoTEdgeDevice()
        {
        }
    }
}
