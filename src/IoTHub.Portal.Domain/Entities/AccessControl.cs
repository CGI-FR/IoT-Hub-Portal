// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class AccessControl : EntityBase
    {
        public string Scope { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public virtual Role? Role { get; set; }
        public virtual User? User { get; set; }
        public virtual Group? Group { get; set; }
        public string? GroupId { get; set; } = default!;
        public string? UserId { get; set; } = default!;
    }
}
