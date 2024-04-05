// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10.Filters
{

    public class RoomFilter : PaginationFilter
    {
        public string Keyword { get; set; } = default!;
    }
}
