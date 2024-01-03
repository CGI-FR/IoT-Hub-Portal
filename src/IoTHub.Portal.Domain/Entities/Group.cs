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
        public string Avatar { get; set; } = default!;
        public virtual ICollection<Member> Members { get; set; } = new Collection<Member>();
        public virtual ICollection<GroupAccessControl> GroupAccessControls { get; set; } = new Collection<GroupAccessControl>();
    }
}
