// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Exceptions
{
    public class InvalidCloudProviderException : BaseException
    {
        public InvalidCloudProviderException(string detail, Exception? innerException = null) : base(ErrorTitles.InvalidCloudProvider, detail, innerException)
        {
        }
    }
}
