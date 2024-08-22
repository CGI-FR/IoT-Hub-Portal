// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.ComponentModel.DataAnnotations;

    public class LabelDto
    {
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Color { get; set; } = default!;
    }
}
