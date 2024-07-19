// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace IoTHub.Portal.Shared.Models.v10
{
    using System;
    using System.Collections.Generic;

    public class PaginatedResult<T>
    {

        public PaginatedResult(List<T>? data = default, int count = 0, int page = 0, int pageSize = 10)
        {
            Data = data;
            CurrentPage = page;
            PageSize = pageSize;
            TotalCount = count;
        }

        public List<T>? Data { get; set; }

        public int CurrentPage { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => CurrentPage > 0;

        public bool HasNextPage => CurrentPage < (TotalPages - 1);
    }
}
