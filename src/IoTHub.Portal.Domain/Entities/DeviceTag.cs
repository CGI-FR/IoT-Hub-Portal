// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class DeviceTag : EntityBase
    {
        [NotMapped] public string Name => Id;

        public string Label { get; set; } = default!;

        public bool Required { get; set; }

        public bool Searchable { get; set; }
    }
}
