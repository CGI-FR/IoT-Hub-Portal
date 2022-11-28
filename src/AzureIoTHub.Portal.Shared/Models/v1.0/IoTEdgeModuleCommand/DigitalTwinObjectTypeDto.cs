// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DigitalTwinObjectTypeDto : PayloadDataTypeDto
    {
        [JsonPropertyName("@type")]
        [JsonProperty("@type")]
        public override string Type { get; } = "Object";

        public IEnumerable<DigitalTwinFieldTypeDto> Fileds { get; set; }
    }
}
