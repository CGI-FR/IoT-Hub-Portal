// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    using Newtonsoft.Json;

    /// <summary>
    /// LoRaWAN Concentrator Channel configuration.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// A value indicating whether the channel is enabled.
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// The frequency.
        /// </summary>
        public int Freq { get; set; }

        /// <summary>
        /// The radio.
        /// </summary>
        public int Radio { get; set; }

        /// <summary>
        /// The interface.
        /// </summary>
        public int If { get; set; }

        /// <summary>
        /// The bandwidth.
        /// </summary>
        public int Bandwidth { get; set; }

        /// <summary>
        /// The spread factor.
        /// </summary>
        [JsonProperty("Spread_factor")]
        public int SpreadFactor { get; set; }
    }
}
