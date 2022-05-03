// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Exceptions
{
    using System;
    using AzureIoTHub.Portal.Server.Constants;

    public class InternalServerErrorException : BaseException
    {
        public InternalServerErrorException(string detail, Exception innerException = null) : base(ErrorTitles.InternalServerError, detail, innerException)
        {
        }
    }
}
