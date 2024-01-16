// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IoTHub.Portal.Domain.Base;

    public class User : EntityBase
    {
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string GivenName { get; set; } = default!;
        public string FamilyName { get; set; } = default!;
        public virtual ICollection<UserMemberShip> Groups { get; set; } = new Collection<UserMemberShip>();
        public virtual ICollection<AccessControl> AccessControls { get; set; } = new Collection<AccessControl>();
    }
}
