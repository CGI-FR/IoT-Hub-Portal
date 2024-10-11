// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10.LoRaWAN
{
    /// <summary>
    /// Router configuration.
    /// </summary>
    public class RouterConfig
    {
        /// <summary>
        /// The network identifier.
        /// </summary>
        [JsonPropertyName("NetID")]
        public List<int> NetID { get; set; } = new();

        /// <summary>
        /// The join eui.
        /// </summary>
        [JsonPropertyName("JoinEui")]
        public List<List<string>> JoinEui { get; set; } = new();

        /// <summary>
        /// The region.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; } = default!;

        /// <summary>
        /// The hardware specifications.
        /// </summary>
        [JsonPropertyName("hwspec")]
        public string Hwspec { get; set; } = default!;

        /// <summary>
        /// The frequency range.
        /// </summary>
        [JsonPropertyName("freq_range")]
        public List<int> FreqRange { get; set; } = new();

        /// <summary>
        /// The DRs.
        /// </summary>
        [JsonPropertyName("DRs")]
        public List<List<int>> DRs { get; set; } = new();

        /// <summary>
        /// The SX1301 conf.
        /// </summary>
        [JsonPropertyName("sx1301_conf")]
        public List<Dictionary<string, Channel>> Sx1301Conf { get; set; } = new();

        /// <summary>
        ///   <c>true</c> if nocca; otherwise, <c>false</c>.
        /// </summary>
        [JsonPropertyName("nocca")]
        public bool Nocca { get; set; }

        /// <summary>
        ///   <c>true</c> if nodc; otherwise, <c>false</c>.
        /// </summary>
        [JsonPropertyName("nodc")]
        public bool Nodc { get; set; }
        /// <summary>
        ///   <c>true</c> if nodwell; otherwise, <c>false</c>.
        /// </summary>
        [JsonPropertyName("nodwell")]
        public bool Nodwell { get; set; }
    }
}
