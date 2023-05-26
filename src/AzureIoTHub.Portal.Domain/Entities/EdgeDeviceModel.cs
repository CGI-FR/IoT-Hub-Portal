// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using Base;

    public class EdgeDeviceModel : EntityBase
    {
        public string Name { get; set; } = default!;

        public string? Description { get; set; }
        public string? ExternalIdentifier { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public ICollection<Label> Labels { get; set; } = default!;

    }
}
