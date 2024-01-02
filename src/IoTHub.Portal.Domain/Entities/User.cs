// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Domain.Base;

    public class User : EntityBase
    {
        [Required]
        public string Email { get; set; } = default!;
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public string Forename { get; set; } = default!;
        public virtual ICollection<Member> Members { get; set; } = new Collection<Member>();
        public virtual ICollection<UserAccessControl> UserAccessControls { get; set; } = new Collection<UserAccessControl>();
    }
}
