// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using IoTHub.Portal.Domain.Base;

    public class Action : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
    }
}
