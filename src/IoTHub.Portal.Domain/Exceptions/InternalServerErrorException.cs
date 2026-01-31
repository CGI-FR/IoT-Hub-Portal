// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Exceptions
{
    public class InternalServerErrorException : BaseException
    {
        public InternalServerErrorException(string detail, Exception? innerException = null) : base(ErrorTitles.InternalServerError, detail, innerException)
        {
        }
    }
}
