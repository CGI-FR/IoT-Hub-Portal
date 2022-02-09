// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator
{
    public class Channel
    {
        public bool? Enable { get; set; }

        public int Freq { get; set; }

        public int Radio { get; set; }

        public int If { get; set; }

        public int Bandwidth { get; set; }

        public int Spread_factor { get; set; }
    }
}
