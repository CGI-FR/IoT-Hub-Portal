// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System.ComponentModel.DataAnnotations;

    public class IdeaRequest
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!;

        public bool ConsentToCollectTechnicalDetails { get; set; }
    }
}
