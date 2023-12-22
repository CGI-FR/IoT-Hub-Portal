// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System;
    using IoTHub.Portal.Domain.Base;

    public class GroupAccessControl : EntityBase
    {
        public Guid GroupId { get; set; } = default!;
        public virtual Group Group { get; set; } = default!;

        public Guid AccessControlId { get; set; } = default!;
        public virtual AccessControl AccessControl { get; set; } = default!;
    }

}
