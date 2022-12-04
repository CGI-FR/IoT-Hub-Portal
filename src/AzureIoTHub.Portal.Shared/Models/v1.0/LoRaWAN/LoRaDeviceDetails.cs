// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Newtonsoft.Json;

    /// <summary>
    /// LoRa WAN Device details.
    /// </summary>
    public class LoRaDeviceDetails : LoRaDeviceBase, IDeviceDetails
    {
        /// <summary>
        /// The name of the device.
        /// </summary>
        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// The model identifier.
        /// </summary>
        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; }

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
        public Dictionary<string, string> Tags { get; set; } = new();

        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required(ErrorMessage = "The device should have a unique identifier.")]
        [MaxLength(ErrorMessage = "The device identifier should be up to 128 characters long.")]
        [RegularExpression("^[A-Z0-9]{16}$", ErrorMessage = "The device identifier must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceID { get; set; }

        /// <summary>
        /// A value indicating whether the device uses OTAA to authenticate to LoRaWAN Network, otherwise ABP
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseOTAA { get; set; }

        /// <summary>
        /// The OTAA App Key.
        /// </summary>
        // [Required(ErrorMessage = "The LoRa Device OTAA App Key is required.")]
        public string AppKey { get; set; }

        /// <summary>
        /// The device OTAA Application EUI.
        /// </summary>
        // [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        ///  The ABP AppSKey.
        /// </summary>
        public string AppSKey { get; set; }

        /// <summary>
        ///  The ABP NwkSKey.
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

        /// <summary>
        /// The GatewayID of the device.
        /// </summary>
        public string GatewayID { get; set; }

        /// <summary>
        /// A value indicating whether the downlinks are enabled (True if not provided)
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool? Downlink { get; set; }

        /// <summary>
        /// A value indicating whether the device supports LoRaWAN features.
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public static bool IsLoraWan => true;

        /// <summary>
        /// Labels
        /// </summary>
        public List<LabelDto> Labels { get; set; } = new();

        public LoRaDeviceDetails()
        {
            Downlink = true;
            PreferredWindow = 1;
            Deduplication = DeduplicationMode.None;
            ABPRelaxMode = true;
        }
    }
}
