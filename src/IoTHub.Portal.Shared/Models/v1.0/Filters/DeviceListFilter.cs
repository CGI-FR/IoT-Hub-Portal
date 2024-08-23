// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10.Filters
{
    using System.Collections.Generic;

    public class DeviceListFilter : PaginationFilter
    {
        public string Keyword { get; set; } = default!;

        public bool? IsEnabled { get; set; }

        public bool? IsConnected { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public string ModelId { get; set; } = default!;

        public List<string> Labels { get; set; } = new();

        public string? LayerId { get; set; } = default!;
    }
}
