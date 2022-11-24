// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DigitalTwinObjectType : PayloadDataType
    {
        [JsonPropertyName("@type")]
        [JsonProperty("@type")]
        public string Type { get; set; }

        public IEnumerable<DigitalTwinFieldType> Fileds { get; set; }
    }
}
