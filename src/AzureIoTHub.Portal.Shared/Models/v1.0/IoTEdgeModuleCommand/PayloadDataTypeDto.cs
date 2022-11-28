// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using JsonSubTypes;
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    [Newtonsoft.Json.JsonConverter(typeof(JsonSubtypes), "@type")]
    [JsonSubtypes.KnownSubType(typeof(DigitalTwinObjectTypeDto), "Object")]
    [JsonSubtypes.KnownSubType(typeof(DigitalTwinEnumTypeDto), "Enum")]
    public class PayloadDataTypeDto
    {
        [JsonPropertyName("@type")]
        [JsonProperty("@type")]
        public virtual string Type { get; }

        [JsonPropertyName("@id")]
        [JsonProperty("@id")]
        public string? Id { get; set; }

        public string? Comment { get; set; }

        public string? Description { get; set; }

        public string? DisplayName { get; set; }
    }
}
