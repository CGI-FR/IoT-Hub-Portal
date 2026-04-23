// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class MenuEntryDto
    {
        public string Id { get; set; } = default!;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "URL is required")]
        [Url(ErrorMessage = "URL must be a valid format")]
        public string Url { get; set; } = default!;

        public int Order { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
