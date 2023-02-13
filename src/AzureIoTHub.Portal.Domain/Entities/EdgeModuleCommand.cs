// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using AzureIoTHub.Portal.Domain.Base;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public class EdgeModuleCommand : EntityBase
    {
        public string Type { get; }

        public string Name { get; set; }

        public string EdgeModelId { get; set; }

        public string EdgeModuleName { get; set; }

        public string? Comment { get; set; }

        public string? Description { get; set; }

        public string? DisplayName { get; set; }

        public string? CommandType { get; set; }

        public EdgeModuleCommandPayload? Request { get; set; }

        public EdgeModuleCommandPayload? Response { get; set; }
    }

    public class EdgeModuleCommandPayload
    {
        public string Name { get; set; }

        public ModuleCommandSchemaType moduleCommandSchemaType { get; set; }

        public object? ComplexSchema { get; set; }

        public string EdgeModuleCommandId { get; set; }

        public string? InitialValue { get; set; }

        public string? Comment { get; set; }

        public string? Description { get; set; }

        public string? DisplayName { get; set; }
    }
}
