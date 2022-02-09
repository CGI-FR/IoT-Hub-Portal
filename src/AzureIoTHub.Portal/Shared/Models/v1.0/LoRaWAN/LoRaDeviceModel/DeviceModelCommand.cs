// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceModelCommand
    {
        /// <summary>
        /// The command name.
        /// </summary>
        [Required(ErrorMessage = "The command name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// The command frame in hexa.
        /// </summary>
        [Required(ErrorMessage = "The frame is required.")]
        public string Frame { get; set; }

        /// <summary>
        /// The LoRa WAN port.
        /// </summary>
        [Required(ErrorMessage = "The port number is required.")]
        [Range(1, 223, ErrorMessage = "The port number should be between 1 and 223.")]
        public int Port { get; set; } = 1;

        /// <summary>
        /// A value indicating whether this instance is builtin.
        /// </summary>
        public bool IsBuiltin { get; set; }
    }
}
