// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.Filters
{

    public class DeviceModelFilter : PaginationFilter
    {
        public string SearchText { get; set; } = default!;
    }
}
