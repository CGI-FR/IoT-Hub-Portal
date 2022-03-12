// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN.LoRaDevice
{
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;

    public class LoRaDeviceDetails : LoRaDeviceBase
    {
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
        /// A value indicating whether the device has already joined the platform.
        /// </summary>
        public bool AlreadyLoggedInOnce { get; set; }

        /// <summary>
        /// The Device Current Datarate,
        /// This value will be only reported if you are using Adaptive Data Rate.
        /// </summary>
        public string DataRate { get; set; }

        /// <summary>
        /// The Device Current Transmit Power,
        /// This value will be only reported if you are using Adaptive Data Rate.
        /// </summary>
        public string TxPower { get; set; }

        /// <summary>
        /// The Device Current repetition when transmitting.
        /// E.g. if set to two, the device will transmit twice his upstream messages.
        /// This value will be only reported if you are using Adaptive Data Rate.
        /// </summary>
        public string NbRep { get; set; }

        /// <summary>
        /// The Device Current Rx2Datarate.
        /// </summary>
        public string ReportedRX2DataRate { get; set; }

        /// <summary>
        /// The Device Current RX1DROffset.
        /// </summary>
        public string ReportedRX1DROffset { get; set; }

        /// <summary>
        /// The Device Current RXDelay.
        /// </summary>
        public string ReportedRXDelay { get; set; }
    }
}
