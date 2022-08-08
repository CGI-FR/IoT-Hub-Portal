// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    using System.Text.Json.Serialization;

    public class LoRaCloudToDeviceMessage
    {
        [JsonPropertyName("rawPayload")]
        public string RawPayload { get; set; }

        [JsonPropertyName("fport")]
        public int Fport { get; set; }

        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }
    }
}
