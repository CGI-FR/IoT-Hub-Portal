// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    using System;
    using IoTHub.Portal.Domain.Base;

    public class MenuEntry : EntityBase
    {
        public string Name { get; set; } = default!;

        public string Url { get; set; } = default!;

        public int Order { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
