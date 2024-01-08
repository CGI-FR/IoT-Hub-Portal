// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10
{
    using System.Collections.Generic;

    public class RoleDetailsModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<ActionModel> Actions { get; set; } = new List<ActionModel>();
    }
}
