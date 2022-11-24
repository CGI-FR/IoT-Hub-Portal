// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class DigitalTwinEnumType : PayloadDataType
    {
        [JsonPropertyName("@type")]
        [JsonProperty("@type")]
        public string Type { get; set; }

        public IEnumerable<DigitalTwinEnumValueType> EnumValues { get; set; }

        public string valueSchema { get; set; }
    }
}
