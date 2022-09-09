// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Exceptions
{
    using System;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Shared.Constants;
    using FluentAssertions;
    using NUnit.Framework;

    public class InternalServerErrorExceptionTests
    {
        [Test]
        public void InternalServerErrorExceptionParametersMustBeCorrect()
        {
            // Act
            var message = Guid.NewGuid().ToString();
            var innerException = new Exception(Guid.NewGuid().ToString());

            // Act
            var exception = new InternalServerErrorException(message, innerException);

            // Assert
            _ = exception.Title.Should().Be(ErrorTitles.InternalServerError);
            _ = exception.Detail.Should().Be(message);
            _ = exception.InnerException.Should().Be(innerException);
        }
    }
}
