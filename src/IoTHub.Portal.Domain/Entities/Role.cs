// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Domain.Base;

    public class Role : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
        public string Color { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public virtual ICollection<Action> Actions { get; set; } = new Collection<Action>();
    }
}
