// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using AzureIoTHub.Portal.Domain.Base;

    public class EdgeModuleCommand : EntityBase
    {
        public string Type { get; }

        public string Name { get; set; }

        public string EdgeModelId { get; set; }

        public string EdgeModuleName { get; set; }

        public string? Description { get; set; }

        public string? DisplayName { get; set; }

        public string? CommandType { get; set; }
    }
}
