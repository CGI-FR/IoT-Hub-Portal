// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.AWS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ThingTypeDto
    {
        public string ThingTypeID { get; set; }

        [Required(ErrorMessage = "The thing type should have a name.")]
        public string ThingTypeName { get; set; }
        public string ThingTypeDescription { get; set; }
        public List<ThingTypeTagDto> Tags { get; set; }
        public List<ThingTypeSearchableAttDto> ThingTypeSearchableAttDtos { get; set; }
        public Uri ImageUrl { get; set; } = default!;

    }
}
