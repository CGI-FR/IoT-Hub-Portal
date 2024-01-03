// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IoTHub.Portal.Domain.Base;

    public class Role : EntityBase
    {
        public string Name { get; set; } = default!;
        public string Avatar { get; set; } = default!;
        public virtual ICollection<RoleAction> RoleActions { get; set; } = new Collection<RoleAction>();
        public virtual ICollection<AccessControl>? AccessControls { get; set; } = new Collection<AccessControl>();
    }
}
