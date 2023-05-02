// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.AWS
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ThingTypeDto
    {
        [Required(ErrorMessage = "The thing type should have a unique identifier.")]
        [MaxLength(ErrorMessage = "The thing type identifier should be up to 128 characters long.")]
        [RegularExpression("^[a-zA-Z0-9\\-.+%_#*?!(),:=@$']{1,128}$", ErrorMessage = "The thing type identifier should be of ASCII 7-bit alphanumeric characters plus certain special characters: - . + % _ # * ? ! ( ) , : = @ $ '.")]
        public virtual string ThingTypeID { get; set; }

        [Required(ErrorMessage = "The thing type should have a name.")]
        public string ThingTypeName { get; set; }
        public string ThingTypeDescription { get; set; }
        public List<ThingTypeTagDto> Tags { get; set; }
        public List<ThingTypeSearchableAttDto> ThingTypeSearchableAttDtos { get; set; }

    }
}
