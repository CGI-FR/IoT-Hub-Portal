// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN.Concentrator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class RouterConfig
    {
        /// <summary>
        /// The network identifier.
        /// </summary>
        public List<int> NetID { get; set; }

        /// <summary>
        /// The join eui.
        /// </summary>
        public List<List<string>> JoinEui { get; set; }

        /// <summary>
        /// The region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The hardware specifications.
        /// </summary>
        public string Hwspec { get; set; }

        /// <summary>
        /// The frequency range.
        /// </summary>
        [JsonProperty("Freq_range")]
        public List<int> FreqRange { get; set; }

        /// <summary>
        /// The DRs.
        /// </summary>
        public List<List<int>> DRs { get; set; }

        /// <summary>
        /// The SX1301 conf.
        /// </summary>
        [JsonProperty("Sx1301_conf")]
        public List<Dictionary<string, Channel>> Sx1301Conf { get; set; }

        /// <summary>
        ///   <c>true</c> if nocca; otherwise, <c>false</c>.
        /// </summary>
        public bool Nocca { get; set; }

        /// <summary>
        ///   <c>true</c> if nodc; otherwise, <c>false</c>.
        /// </summary>
        public bool Nodc { get; set; }

        /// <summary>
        ///   <c>true</c> if nodwell; otherwise, <c>false</c>.
        /// </summary>
        public bool Nodwell { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouterConfig"/> class.
        /// </summary>
        public RouterConfig()
        {
            this.Region = string.Empty;
            this.Hwspec = string.Empty;
            this.FreqRange = new List<int>();
            this.NetID = new List<int>();
            this.JoinEui = new List<List<string>>();
            this.DRs = new List<List<int>>();
            this.Sx1301Conf = new List<Dictionary<string, Channel>>();
        }
    }
}
