// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator
{
    using System.Collections.Generic;

    public class RouterConfig
    {
        public List<int> NetID { get; set; }

        public List<List<string>> JoinEui { get; set; }

        public string Region { get; set; }

        public string Hwspec { get; set; }

        public List<int> Freq_range { get; set; }

        public List<List<int>> DRs { get; set; }

        public List<Dictionary<string, Channel>> Sx1301_conf { get; set; }

        public bool Nocca { get; set; }

        public bool Nodc { get; set; }

        public bool Nodwell { get; set; }

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
