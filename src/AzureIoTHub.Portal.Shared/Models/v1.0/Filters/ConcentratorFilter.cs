// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10.Filters
{

    public class ConcentratorFilter : PaginationFilter
    {
        public string Keyword { get; set; }

        public bool? Status { get; set; }

        public bool? State { get; set; }
    }
}
