// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.Filters
{
    public class EdgeDeviceListFilter : PaginationFilter
    {
        public string Keyword { get; set; }

        public bool? IsEnabled { get; set; }

        public string ModelId { get; set; }
    }
}
