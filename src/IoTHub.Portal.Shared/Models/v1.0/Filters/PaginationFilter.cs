// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.Filters
{
    public abstract class PaginationFilter
    {
        protected PaginationFilter()
        {
            PageSize = int.MaxValue;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string[] OrderBy { get; set; } = new string[]
        {
            string.Empty
        };
    }
}
