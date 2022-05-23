// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Exceptions;
    using Client.Handlers;
    using Client.Models;
    using FluentAssertions;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    public class ProblemDetailsHandlerTests
    {
        [Test]
        public async Task HttpClientShouldReturnsResponseWhenProblemDetailsHandlerReturnsResponse()
        {
            // Arrange
            var mockProblemDetailsHandler = new Mock<ProblemDetailsHandler>();

            _ = mockProblemDetailsHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("test")
                });

            var httpClient = new HttpClient(mockProblemDetailsHandler.Object)
            {
                BaseAddress = new Uri("http://fake.com")
            };

            // Act
            var result = await httpClient.GetStringAsync("");

            // Assert
            _ = result.Should().Be("test");
            mockProblemDetailsHandler.VerifyAll();
        }

        [Test]
        public async Task HttpClientShouldThrowsProblemDetailsExceptionWhenProblemDetailsHandlerThrowsProblemDetailsException()
        {
            // Arrange
            var problemDetailsException = new ProblemDetailsException(new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 400,
                TraceId = "traceId",
                ExceptionDetails = new List<ProblemDetailsWithExceptionDetails.ExceptionDetail>
                {
                    new()
                    {
                        Message = "message",
                        Raw = "raw",
                        Type = "type",
                        StackFrames = new List<ProblemDetailsWithExceptionDetails.StackFrame>()
                        {
                            new()
                            {
                                ContextCode = new List<string>(),
                                FileName = "fileName",
                                Line = 0,
                                FilePath = "filePath",
                                Function = "function",
                                PostContextCode = new List<string>(),
                                PreContextCode = new List<string>(),
                                PreContextLine = 1
                            }
                        }
                    }
                }
            });


            var mockProblemDetailsHandler = new Mock<ProblemDetailsHandler>();

            _ = mockProblemDetailsHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(problemDetailsException);

            var httpClient = new HttpClient(mockProblemDetailsHandler.Object)
            {
                BaseAddress = new Uri("http://fake.com")
            };

            // Act
            var result = () => httpClient.GetStringAsync("");

            // Assert
            var exceptionAssertions = await result.Should().ThrowAsync<ProblemDetailsException>();
            _ = exceptionAssertions.Which.Should().BeEquivalentTo(problemDetailsException);
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Should().BeEquivalentTo(problemDetailsException.ProblemDetailsWithExceptionDetails);
            mockProblemDetailsHandler.VerifyAll();
        }
    }
}
