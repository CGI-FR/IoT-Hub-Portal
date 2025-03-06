// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10.LoRaWAN
{
    /// <summary>
    /// LoRaWAN Concentrator Channel configuration.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// A value indicating whether the channel is enabled.
        /// </summary>
        [JsonPropertyName("enable")]
        [JsonProperty("enable")]
        public bool? Enable { get; set; }

        /// <summary>
        /// The frequency.
        /// </summary>
        [JsonPropertyName("freq")]
        [JsonProperty("freq")]
        public int Freq { get; set; }

        /// <summary>
        /// The radio.
        /// </summary>
        [JsonPropertyName("radio")]
        [JsonProperty("radio")]
        public int Radio { get; set; }

        /// <summary>
        /// The interface.
        /// </summary>
        [JsonPropertyName("if")]
        [JsonProperty("if")]
        public int If { get; set; }

        /// <summary>
        /// The bandwidth.
        /// </summary>
        [JsonPropertyName("bandwidth")]
        [JsonProperty("bandwidth")]
        public int Bandwidth { get; set; }

        /// <summary>
        /// The spread factor.
        /// </summary>
        [JsonPropertyName("spread_factor")]
        [JsonProperty("spread_factor")]
        public int SpreadFactor { get; set; }
    }
}
