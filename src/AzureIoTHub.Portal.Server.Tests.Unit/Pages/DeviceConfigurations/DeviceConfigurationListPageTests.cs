// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DeviceConfigurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.DeviceConfigurations;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceConfigurationListPageTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void DeviceConfigurationListPageShouldLoadAndShowConfigurations()
        {
            // Arrange

            _ = MockHttpClient
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
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void DeviceConfigurationListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConfigurations()
        {
            // Arrange

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/device-configurations")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Find("#device-configurations-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(2);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
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
                .When(HttpMethod.Get, "/api/device-configurations")
                .RespondJson(configurations);

            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/device-configurations/{configurationId}"));
        }
    }
}
