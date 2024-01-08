// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class AccessControl : EntityBase
    {
        public string Scope { get; set; } = default!;
        public string RoleId { get; set; } = default!;
        public Role Role { get; set; } = new Role();
    }
}
