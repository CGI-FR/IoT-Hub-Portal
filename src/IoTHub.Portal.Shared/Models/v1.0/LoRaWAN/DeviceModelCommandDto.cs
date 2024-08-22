// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.LoRaWAN
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Device model command.
    /// </summary>
    public class DeviceModelCommandDto
    {
        /// <summary>
        /// The command identifier.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The command name.
        /// </summary>
        [Required(ErrorMessage = "The command name is required.")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The command frame in hexa.
        /// </summary>
        [Required(ErrorMessage = "The frame is required.")]
        [MaxLength(255, ErrorMessage = "The frame should be up to 255 characters long.")]
        [RegularExpression("^[0-9a-fA-F]{0,255}$", ErrorMessage = "The frame should only contain hexadecimal characters")]
        public string Frame { get; set; } = default!;

        /// <summary>
        /// A value indicating if the command must be confirmed by sensor
        /// </summary>
        public bool Confirmed { get; set; }

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
