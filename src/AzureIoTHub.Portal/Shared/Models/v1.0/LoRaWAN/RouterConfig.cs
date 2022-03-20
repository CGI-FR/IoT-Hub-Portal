// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// Router configuration.
    /// </summary>
    public class RouterConfig
    {
        /// <summary>
        /// The network identifier.
        /// </summary>
        public Collection<int> NetID { get; }

        /// <summary>
        /// The join eui.
        /// </summary>
        public Collection<Collection<string>> JoinEui { get; }

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
        public Collection<int> FreqRange { get; }

        /// <summary>
        /// The DRs.
        /// </summary>
        public Collection<Collection<int>> DRs { get; }

        /// <summary>
        /// The SX1301 conf.
        /// </summary>
        [JsonProperty("Sx1301_conf")]
        public Collection<Dictionary<string, Channel>> Sx1301Conf { get; }

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
            Region = string.Empty;
            Hwspec = string.Empty;
            FreqRange = new Collection<int>();
            NetID = new Collection<int>();
            JoinEui = new Collection<Collection<string>>();
            DRs = new Collection<Collection<int>>();
            Sx1301Conf = new Collection<Dictionary<string, Channel>>();
        }
    }
}
