// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator
{
    using System.ComponentModel.DataAnnotations;

    public class Concentrator
    {
        [Required]
        [RegularExpression("^[A-F0-9]{16}$", ErrorMessage = "DeviceID must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceId { get; set; }

        [Required]
        public string DeviceName { get; set; }

        [Required]
        public string LoraRegion { get; set; }

        public string DeviceType { get; set; }

        public string ClientCertificateThumbprint { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public bool AlreadyLoggedInOnce { get; set; }

        public RouterConfig RouterConfig { get; set; }
    }
}
