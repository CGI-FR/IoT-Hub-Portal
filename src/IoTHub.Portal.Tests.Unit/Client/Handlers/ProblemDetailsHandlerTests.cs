// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Handlers;
    using IoTHub.Portal.Client.Models;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit.TestDoubles;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using System.Linq;
    using Bunit;

    [TestFixture]
    public class ProblemDetailsHandlerTests : BlazorUnitTest
    {
        private ISnackbar snackBarService;
        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.snackBarService = Services.GetRequiredService<ISnackbar>();
            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public async Task HttpClientShouldReturnsResponseWhenProblemDetailsHandlerReturnsResponse1()
        {
            // Arrange
            using var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Get, "http://fake.com")
                .Respond(MediaTypeNames.Application.Json, "test");

            var problemDetailsHandler = new ProblemDetailsHandler(snackBarService, mockNavigationManager)
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
                TraceId = "traceId"
            };

            using var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Get, "http://fake.com")
                .Respond(System.Net.HttpStatusCode.InternalServerError, new StringContent(
                    JsonConvert.SerializeObject(problemDetailsWithExceptionDetails),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            var problemDetailsHandler = new ProblemDetailsHandler(snackBarService, mockNavigationManager)
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
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Title.Should().Be(problemDetailsWithExceptionDetails.Title);
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Detail.Should().Be(problemDetailsWithExceptionDetails.Detail);
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Status.Should().Be(problemDetailsWithExceptionDetails.Status);
        }

        [Test]
        public async Task ProblemDetailsHandler_UnauthorizedHttpStatusCode_RedirectToLoginSnackBarIsCreatedAndProblemDetailsExceptionIsThrown()
        {
            // Arrange
            var problemDetailsWithExceptionDetails = new ProblemDetailsWithExceptionDetails
            {
                Title = "title",
                Detail = "detail",
                Status = 401,
                TraceId = "traceId",
                ExceptionDetails = new List<ProblemDetailsWithExceptionDetails.ExceptionDetail>()
            };

            using var mockHttp = new MockHttpMessageHandler();

            _ = mockHttp.When(HttpMethod.Get, "http://fake.com")
                .Respond(System.Net.HttpStatusCode.Unauthorized, new StringContent(
                    JsonConvert.SerializeObject(problemDetailsWithExceptionDetails),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            var problemDetailsHandler = new ProblemDetailsHandler(snackBarService, mockNavigationManager)
            {
                InnerHandler = mockHttp
            };

            var httpClient = new HttpClient(problemDetailsHandler)
            {
                BaseAddress = new Uri("http://fake.com")
            };

            var mudSnackbarProvider = RenderComponent<MudSnackbarProvider>();

            // Act
            var result = () => httpClient.GetStringAsync("");

            // Assert
            var exceptionAssertions = await result.Should().ThrowAsync<ProblemDetailsException>();
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Title.Should().Be(problemDetailsWithExceptionDetails.Title);
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Detail.Should().Be(problemDetailsWithExceptionDetails.Detail);
            _ = exceptionAssertions.Which.ProblemDetailsWithExceptionDetails.Status.Should().Be(problemDetailsWithExceptionDetails.Status);

            _ = snackBarService.ShownSnackbars.Count().Should().Be(1);

            var errorSnackBar = snackBarService.ShownSnackbars.First();

            _ = errorSnackBar.Severity.Should().Be(Severity.Error);
            _ = errorSnackBar.Message.Should().Be("You are not authorized");

            mudSnackbarProvider.Find("button").Click();

            mudSnackbarProvider.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/authentication/login?returnUrl="));
        }
    }
}
