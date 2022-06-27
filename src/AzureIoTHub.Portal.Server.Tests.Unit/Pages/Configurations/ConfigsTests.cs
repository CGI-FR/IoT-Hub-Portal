// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConfigsTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });
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

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/edge/configurations")
                .RespondJson(configurations);

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(3));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [TestCase]
        public void ConfigsPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfigurations()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/edge/configurations")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
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

            _ = MockHttpClient
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
