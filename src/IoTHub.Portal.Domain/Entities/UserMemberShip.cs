// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class UserMemberShip
    {
        public string UserId { get; set; } = default!;
        public virtual User? User { get; set; }

        public string GroupId { get; set; } = default!;
        public virtual Group? Group { get; set; }
    }
}
