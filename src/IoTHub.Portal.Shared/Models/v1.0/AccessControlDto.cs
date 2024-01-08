// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;

    public class AccessControlDto
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public string RoleId { get; set; }

        public static implicit operator string(AccessControlDto v) => throw new NotImplementedException();
    }
}
