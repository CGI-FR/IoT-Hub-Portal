// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System.Collections.ObjectModel;
    using IoTHub.Portal.Domain.Base;

    public class Action : EntityBase
    {
        public string Name { get; set; } = default!;
        public string ApiEndpoint { get; set; } = default!; //"/api/v1/..."
        public string HttpMethod { get; set; } = default!; //"GET", "POST", "PUT", "DELETE"...
        public virtual ICollection<RoleAction> RoleActions { get; set; } = new Collection<RoleAction>();
    }
}
