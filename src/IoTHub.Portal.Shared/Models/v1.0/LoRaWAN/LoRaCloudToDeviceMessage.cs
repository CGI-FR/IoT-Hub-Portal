// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.LoRaWAN
{
    using System.Text.Json.Serialization;

    public class LoRaCloudToDeviceMessage
    {
        [JsonPropertyName("rawPayload")]
        public string RawPayload { get; set; } = default!;

        [JsonPropertyName("fport")]
        public int Fport { get; set; }

        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }
    }
}
