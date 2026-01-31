// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Exceptions
{
    public class IncompatibleDeviceModelException : BaseException
    {
        public IncompatibleDeviceModelException(string detail, Exception? innerException = null) : base(ErrorTitles.IncompatibleDeviceModel, detail, innerException)
        {
        }
    }
}
