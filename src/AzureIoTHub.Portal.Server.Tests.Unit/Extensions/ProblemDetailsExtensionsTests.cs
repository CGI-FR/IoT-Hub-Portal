// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Extensions
{
    using Client.Extensions;
    using Client.Models;
    using FluentAssertions;
    using NUnit.Framework;

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

            const string expectedJsonStringResult = /*lang=json,strict*/ "{\r\n  \"TraceId\": \"traceId\",\r\n  \"ExceptionDetails\": null,\r\n  \"Type\": null,\r\n  \"Title\": \"title\",\r\n  \"Status\": 400,\r\n  \"Detail\": \"detail\",\r\n  \"Instance\": null,\r\n  \"Extensions\": {}\r\n}";

            // Act
            var result = problemDetailsWithExceptionDetails.ToJson();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedJsonStringResult);
        }
    }
}
