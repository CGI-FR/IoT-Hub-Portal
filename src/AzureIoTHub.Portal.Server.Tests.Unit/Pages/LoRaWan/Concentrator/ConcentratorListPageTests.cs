// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Helpers;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConcentratorListPageTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;

        [SetUp]
        public void SetUp()
        {
            TestContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();

            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            _ = TestContext.Services.AddSingleton(this.mockDialogService.Object);

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = TestContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = TestContext.Services.AddMudServices();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
            this.mockHttpClient.AutoFlush = true;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void ConcentratorListPageShouldLoadAndShowConcentrators()
        {
            // Arrange

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/lorawan/concentrators?pageSize=10")
                .RespondJson(new PaginationResult<Concentrator>
                {
                    Items = new List<Concentrator>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            // Act
            var cut = RenderComponent<ConcentratorListPage>();
            cut.WaitForAssertion(() => cut.Find("#concentrators-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(4);
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ConcentratorListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConcentrators()
        {
            // Arrange

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/lorawan/concentrators?pageSize=10")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<ConcentratorListPage>();
            cut.WaitForAssertion(() => cut.Find("#concentrators-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(2);
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
