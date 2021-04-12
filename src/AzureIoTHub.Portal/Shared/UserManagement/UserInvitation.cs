// Copyright (c) Kevin BEAUGRAND. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.UserManagement
{
    using System.ComponentModel.DataAnnotations;

    public class UserInvitation
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string UserDisplayName { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
