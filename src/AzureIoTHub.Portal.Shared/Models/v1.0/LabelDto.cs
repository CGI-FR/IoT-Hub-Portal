// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    using System.ComponentModel.DataAnnotations;

    public class LabelDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }
    }
}
