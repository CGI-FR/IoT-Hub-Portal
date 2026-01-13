// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Base;

    public class User : EntityBase
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string GivenName { get; set; } = default!;
        public string? Name { get; set; } = default!;
        public string? FamilyName { get; set; } = default!;
        public string? Avatar { get; set; } = default!;
        public string PrincipalId { get; set; }
    }
}
