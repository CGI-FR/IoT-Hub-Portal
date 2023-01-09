// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.Filters
{
    public abstract class PaginationFilter
    {
        protected PaginationFilter()
        {
            PageSize = int.MaxValue;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string[] OrderBy { get; set; }
    }
}
