// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Extensions
{
    using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

    public class ProblemDetailsOptionsExtensionsTests
    {
        [Test]
        public void MapFluentValidationExceptionShouldNotAlterProblemDetailsOptions()
        {
            // Arrange
            var options = new ProblemDetailsOptions();

            // Act
            options.MapFluentValidationException();

            // Assert
            _ = options.Should().NotBeNull();
            _ = options.ValidationProblemStatusCode.Should().Be(422);
            _ = options.ExceptionDetailsPropertyName.Should().Be("exceptionDetails");
            _ = options.TraceIdPropertyName.Should().Be("traceId");
        }
    }
}
