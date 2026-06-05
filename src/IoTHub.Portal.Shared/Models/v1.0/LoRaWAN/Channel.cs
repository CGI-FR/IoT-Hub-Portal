// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10.LoRaWAN
{
    using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

    /// <summary>
    /// LoRaWAN Concentrator Channel configuration.
    /// Covers all SX1301_conf entry types: radio_*, chan_multiSF_*, chan_Lora_std, and chan_FSK.
    /// Fields absent in a given entry type deserialize as null and are omitted on re-serialization.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Human-readable description of the channel.
        /// </summary>
        [JsonPropertyName("desc")]
        [JsonProperty("desc", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Desc { get; set; }

        /// <summary>
        /// A value indicating whether the channel is enabled.
        /// </summary>
        [JsonPropertyName("enable")]
        [JsonProperty("enable", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Enable { get; set; }

        /// <summary>
        /// The centre frequency (Hz). Present on radio_* entries only.
        /// </summary>
        [JsonPropertyName("freq")]
        [JsonProperty("freq", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Freq { get; set; }

        /// <summary>
        /// Whether TX is enabled on this radio. Present on radio_* entries only.
        /// </summary>
        [JsonPropertyName("tx_enable")]
        [JsonProperty("tx_enable", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? TxEnable { get; set; }

        /// <summary>
        /// Minimum TX frequency (Hz). Present on radio_* entries only.
        /// </summary>
        [JsonPropertyName("tx_freq_min")]
        [JsonProperty("tx_freq_min", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TxFreqMin { get; set; }

        /// <summary>
        /// Maximum TX frequency (Hz). Present on radio_* entries only.
        /// </summary>
        [JsonPropertyName("tx_freq_max")]
        [JsonProperty("tx_freq_max", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TxFreqMax { get; set; }

        /// <summary>
        /// The radio index (0 or 1). Present on chan_* entries only.
        /// </summary>
        [JsonPropertyName("radio")]
        [JsonProperty("radio", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Radio { get; set; }

        /// <summary>
        /// The IF frequency offset (Hz). Present on chan_* entries only.
        /// </summary>
        [JsonPropertyName("if")]
        [JsonProperty("if", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? If { get; set; }

        /// <summary>
        /// The bandwidth (Hz). Present on chan_Lora_std and chan_FSK entries.
        /// </summary>
        [JsonPropertyName("bandwidth")]
        [JsonProperty("bandwidth", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Bandwidth { get; set; }

        /// <summary>
        /// The FSK data rate (bps). Present on chan_FSK entries only.
        /// </summary>
        [JsonPropertyName("datarate")]
        [JsonProperty("datarate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Datarate { get; set; }

        /// <summary>
        /// The spreading factor. Present on chan_Lora_std entries only.
        /// </summary>
        [JsonPropertyName("spread_factor")]
        [JsonProperty("spread_factor", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SpreadFactor { get; set; }
    }
}
