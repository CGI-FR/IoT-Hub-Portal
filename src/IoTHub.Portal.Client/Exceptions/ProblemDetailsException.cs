// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Exceptions
{
    using System;
    using Models;

    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(ProblemDetailsWithExceptionDetails? problemDetailsWithExceptionDetails)
        {
            ProblemDetailsWithExceptionDetails = problemDetailsWithExceptionDetails ?? new ProblemDetailsWithExceptionDetails();
        }

        public ProblemDetailsWithExceptionDetails ProblemDetailsWithExceptionDetails { get; }
    }
}
