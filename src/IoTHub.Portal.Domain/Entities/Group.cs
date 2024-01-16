// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class Group : EntityBase
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public virtual ICollection<UserMemberShip> Members { get; set; } = new Collection<UserMemberShip>();
        public virtual ICollection<AccessControl> AccessControls { get; set; } = new Collection<AccessControl>();

    }
}
