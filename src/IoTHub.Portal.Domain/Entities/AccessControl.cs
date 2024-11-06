// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class AccessControl : EntityBase
    {
        public Scope Scope { get; set; }
        public string ScopeId { get; set; } = default!;
        public string RoleId { get; set; } = default!;
        public virtual Role Role { get; set; }
        public string PrincipalId { get; set; } = default!;
        public virtual Principal Principal { get; set; }
    }
}
