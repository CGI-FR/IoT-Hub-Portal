// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;

    public class Group : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
        public string Color { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public virtual ICollection<User> Members { get; set; } = new Collection<User>();
        public string PrincipalId { get; set; }
        public virtual Principal Principal { get; set; } = new Principal();

    }
}
