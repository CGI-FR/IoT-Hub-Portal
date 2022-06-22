// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.Configurations;
    using AzureIoTHub.Portal.Server.Tests.Unit.Extensions;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConfigsTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.mockHttpClient.AutoFlush = true;

            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        [TestCase]
        public void ConfigsPageMustLoadConfigurations()
        {
            // Arrange
            var configurations = new List<ConfigListItem>
            {
                new(),
                new()
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/configurations")
                .RespondJson(configurations);

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(3));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [TestCase]
        public void ConfigsPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfigurations()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/configurations")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void ClickToItemShouldRedirectToConfigurationDetailsPage()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();

            var configurations = new List<ConfigListItem>
            {
                new ConfigListItem
                {
                    ConfigurationID = configurationId
                }
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/configurations")
                .RespondJson(configurations);

            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => this.TestContext.Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/edge/configurations/{configurationId}"));
        }
    }
}
