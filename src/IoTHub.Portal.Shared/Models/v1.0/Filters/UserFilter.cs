// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v10.Filters
{

    public class UserFilter : PaginationFilter
    {
        public string SearchName { get; set; } = default!;
        public string SearchEmail { get; set; } = default!;
    }
}
