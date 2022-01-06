// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared
{
    using System.Collections.Generic;

    public class PaginationResult<TItem>
        where TItem : class
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<TItem> Items { get; set; }
    }
}
