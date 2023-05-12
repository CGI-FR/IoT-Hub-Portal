// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities.AWS
{
    using System.ComponentModel.DataAnnotations;
    using AzureIoTHub.Portal.Domain.Base;

    public class ThingType : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public bool Deprecated { get; set; }
        public ICollection<ThingTypeTag>? Tags { get; set; } = default!;
        public ICollection<ThingTypeSearchableAtt>? ThingTypeSearchableAttributes { get; set; } = default!;
    }
}
