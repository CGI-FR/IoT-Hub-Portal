// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.AWS
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ThingTypeDetails
    {
        [Required(ErrorMessage = "The device should have a unique identifier.")]
        [MaxLength(ErrorMessage = "The device identifier should be up to 128 characters long.")]
        [RegularExpression("^[a-zA-Z0-9\\-.+%_#*?!(),:=@$']{1,128}$", ErrorMessage = "The device identifier should be of ASCII 7-bit alphanumeric characters plus certain special characters: - . + % _ # * ? ! ( ) , : = @ $ '.")]
        public virtual string ThingTypeID { get; set; }

        [Required(ErrorMessage = "The device should have a name.")]
        public string ThingTypeName { get; set; }
        public string ThingTypeDescription { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public List<ThingTypeSearchableAttDto> ThingTypeSearchableAttDtos { get; set; }
    }
}
