// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator
{
    using System.Collections.Generic;

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
        public List<int> Freq_range { get; set; }

        /// <summary>
        /// The DRs.
        /// </summary>
        public List<List<int>> DRs { get; set; }

        /// <summary>
        /// The SX1301 conf.
        /// </summary>
        public List<Dictionary<string, Channel>> Sx1301_conf { get; set; }

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
            this.Freq_range = new List<int>();
            this.NetID = new List<int>();
            this.JoinEui = new List<List<string>>();
            this.DRs = new List<List<int>>();
            this.Sx1301_conf = new List<Dictionary<string, Channel>>();
        }
    }
}
