// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DeviceConfigurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.DeviceConfigurations;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DeviceConfigurationListPageTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;

        [SetUp]
        public void SetUp()
        {
            TestContext = new TestContext();

            this.mockHttpClient = TestContext.Services.AddMockHttpClient();

            _ = TestContext.Services.AddMudServices();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
            this.mockHttpClient.AutoFlush = true;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void DeviceConfigurationListPageShouldLoadAndShowConfigurations()
        {
            // Arrange

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/device-configurations")
                .RespondJson(new List<ConfigListItem>
                {
                    new(),
                    new(),
                    new()
                });

            // Act
            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Find("#device-configurations-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(4);
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void DeviceConfigurationListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConfigurations()
        {
            // Arrange

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/device-configurations")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Find("#device-configurations-listing"));
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
