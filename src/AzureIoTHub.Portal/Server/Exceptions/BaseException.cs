// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Exceptions
{
    using System;

    public abstract class BaseException : Exception
    {
        public BaseException(string title, string detail, Exception innerException = null) : base(detail, innerException)
        {
            Title = title;
            Detail = detail;
        }

        public string Title { get; set; }
        public string Detail { get; set; }
    }
}
