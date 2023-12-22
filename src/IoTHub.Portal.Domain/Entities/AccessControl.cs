// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Domain.Base;

    public class AccessControl : EntityBase
    {
        [Required]
        public string Scope { get; set; } = default!;
        [Required]
        public Guid RoleId { get; set; } = default!;
        [Required]
        public virtual Role Role { get; set; } = default!;

        public ICollection<User> Users { get; set; } = new Collection<User>();
    }
}
