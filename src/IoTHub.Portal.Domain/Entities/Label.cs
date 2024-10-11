// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class Label : EntityBase
    {
        public string Name { get; set; } = default!;

        public string Color { get; set; } = default!;
    }
}
