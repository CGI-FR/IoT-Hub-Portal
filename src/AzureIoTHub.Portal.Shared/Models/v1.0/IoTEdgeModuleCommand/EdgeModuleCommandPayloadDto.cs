// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class EdgeModuleCommandPayloadDto
    {
        [MaxLength(ErrorMessage = "The device identifier should be up to 64 characters long.")]
        [RegularExpression("^[a-zA-Z](?:[a-zA-Z0-9_]{0,62}[a-zA-Z0-9])?$")]
        public string Name { get; set; }

        public ModuleCommandSchemaType SchemaType { get; set; }

        public object Schema { get; set; }

        public object? InitialValue { get; set; }

        [JsonPropertyName("@id")]
        [JsonProperty("@id")]
        public string? Id { get; set; }

        public string? Comment { get; set; }

        public string? Description { get; set; }

        public string? DisplayName { get; set; }
    }
}
