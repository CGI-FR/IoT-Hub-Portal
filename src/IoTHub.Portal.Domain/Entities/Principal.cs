// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Domain.Entities
{
    using IoTHub.Portal.Domain.Base;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;

    public class Principal : EntityBase
    {
        [Required]
        public virtual ICollection<AccessControl> AccessControls { get; set; } = new Collection<AccessControl>();
        public virtual User? User { get; set; }
    }
}
