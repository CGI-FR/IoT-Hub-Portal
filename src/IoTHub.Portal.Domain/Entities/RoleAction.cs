// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class RoleAction : EntityBase
    {
        public string RoleId { get; set; } = default!;
        public virtual Role Role { get; set; } = default!;

        public string ActionId { get; set; } = default!;
        public virtual Action Action { get; set; } = default!;
    }
}

