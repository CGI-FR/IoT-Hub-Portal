// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Shared.Models.v1._0.LoRaWAN
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// LoRaWAN Concentrator.
    /// </summary>
    public class ConcentratorDto
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required]
        [RegularExpression("^[A-F0-9]{16}$", ErrorMessage = "DeviceID must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceId { get; set; } = default!;

        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required]
        public string DeviceName { get; set; } = default!;

        /// <summary>
        /// The lora region.
        /// </summary>
        [Required]
        public string LoraRegion { get; set; } = default!;

        /// <summary>
        /// The type of the device.
        /// </summary>
        public string DeviceType { get; set; } = default!;

        /// <summary>
        /// The client certificate thumbprint.
        /// </summary>
        [RegularExpression("^(([A-F0-9]{2}:){19}[A-F0-9]{2}|)$", ErrorMessage = "ClientThumbprint must contain 40 hexadecimal characters")]
        public string? ClientThumbprint { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///   <c>true</c> if [already logged in once]; otherwise, <c>false</c>.
        /// </summary>
        public bool AlreadyLoggedInOnce { get; set; }

        /// <summary>
        /// The router configuration.
        /// </summary>
        public RouterConfig RouterConfig { get; set; } = new RouterConfig();
    }
}
