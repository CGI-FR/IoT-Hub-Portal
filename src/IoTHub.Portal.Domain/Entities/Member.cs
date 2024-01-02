// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;

    public class Member : EntityBase
    {
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid GroupId { get; set; }
        public virtual Group? Group { get; set; }
    }
}
