// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Handlers;
    using AzureIoTHub.Portal.Client.Models;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ProblemDetailsHandlerTests
    {
        [Test]
        public async Task HttpClientShouldReturnsResponseWhenProblemDetailsHandlerReturnsResponse1()
        {
            // Arrange
            using var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Get, "http://fake.com")
                .Respond(MediaTypeNames.Application.Json, "test");

            var problemDetailsHandler = new ProblemDetailsHandler
            {
                InnerHandler = mockHttp
            };

            var httpClient = new HttpClient(problemDetailsHandler)
            {
                BaseAddress = new Uri("http://fake.com")
            };

            // Act
            var result = await httpClient.GetStringAsync("");

            // Assert
            _ = result.Should().Be("test");
        }

        [Test]
        public async Task HttpClientShouldThrowsProblemDetailsExceptionWhenProblemDetailsHandlerThrowsProblemDetailsException()
        {
            // Arrange
            var problemDetailsWithExceptionDetails = new ProblemDetailsWithExceptionDetails
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
            };

            using var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Get, "http://fake.com")
                .Respond(System.Net.HttpStatusCode.InternalServerError, new StringContent(
                    JsonConvert.SerializeObject(problemDetailsWithExceptionDetails),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            var problemDetailsHandler = new ProblemDetailsHandler
            {
                InnerHandler = mockHttp
            };

            var httpClient = new HttpClient(problemDetailsHandler)
            {
                BaseAddress = new Uri("http://fake.com")
            };

            // Act
            var result = () => httpClient.GetStringAsync("");

            // Assert
            var exceptionAssertions = await result.Should().ThrowAsync<ProblemDetailsException>();
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Should().BeEquivalentTo(problemDetailsWithExceptionDetails);
        }
    }
}
