// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal
{
    using System.Collections.Generic;

    /// <summary>
    /// Class representing the page results.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class PaginationResult<TItem>
        where TItem : class
    {
        /// <summary>
        /// The current page items.
        /// </summary>
        public IEnumerable<TItem> Items { get; set; }

        /// <summary>
        /// The total number of items.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The query next page Url.
        /// </summary>
        public string NextPage { get; set; }
    }
}
