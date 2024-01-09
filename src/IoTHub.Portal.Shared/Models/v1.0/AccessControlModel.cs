// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{

    public class AccessControlModel
    {

        public string Id { get; set; }
        public string Scope { get; set; } = default!;
        public RoleModel Role { get; set; } = default!;
    }
}
