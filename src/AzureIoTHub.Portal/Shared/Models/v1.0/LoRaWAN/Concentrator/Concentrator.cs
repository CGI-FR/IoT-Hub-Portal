// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator
{
    using System.ComponentModel.DataAnnotations;

    public class Concentrator
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required]
        [RegularExpression("^[A-F0-9]{16}$", ErrorMessage = "DeviceID must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required]
        public string DeviceName { get; set; }

        /// <summary>
        /// The lora region.
        /// </summary>
        [Required]
        public string LoraRegion { get; set; }

        /// <summary>
        /// The type of the device.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// The client certificate thumbprint.
        /// </summary>
        public string ClientCertificateThumbprint { get; set; }

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
        public RouterConfig RouterConfig { get; set; }
    }
}
