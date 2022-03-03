// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice
{
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using System.ComponentModel.DataAnnotations;

    public class LoRaDeviceDetails : DeviceDetails
    {
        public bool IsOTTAsetting { get; set; }

        /// <summary>
        /// The OTAA App EUI.
        /// </summary>
        // [Required(ErrorMessage = "The LoRa Device OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        /// The OTAA App Key.
        /// </summary>
        // [Required(ErrorMessage = "The LoRa Device OTAA App Key is required.")]
        public string AppKey { get; set; }

        /// <summary>
        ///  The APB AppSKey.
        /// </summary>
        public string AppSKey { get; set; }

        /// <summary>
        ///  The APB NwkSKey.
        /// </summary>
        public string NwkSKey { get; set; }

        /// <summary>
        /// Unique identifier that allows
        /// the device to be recognized.
        /// </summary>
        public string DevAddr { get; set; }

        /// <summary>
        /// The sensor decoder API Url.
        /// </summary>
        public string SensorDecoder { get; set; }

        public string GatewayID { get; set; }

        /// <summary>
        /// A value indicating whether the device has already joined the platform.
        /// </summary>
        public bool AlreadyLoggedInOnce { get; set; }
    }
}