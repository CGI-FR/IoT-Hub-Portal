// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Extensions
{
    [TestFixture]
    public class ProblemDetailsExtensionsTests
    {
        [Test]
        public void ToJsonShouldConvertProblemDetailsWithExceptionDetailsToJsonString()
        {
            // Arrange
            var problemDetailsWithExceptionDetails = new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 400,
                TraceId = "traceId",
            };

            // Act
            var result = problemDetailsWithExceptionDetails.ToJson();

            // Assert
            var deserializeResult = JsonSerializer.Deserialize<ProblemDetailsWithExceptionDetails>(result);
            _ = deserializeResult.Should().BeEquivalentTo(problemDetailsWithExceptionDetails);
        }
    }
}
