// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Configurations
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigsTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceConfigurationsClientService> mockEdgeDeviceConfigurationsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceConfigurationsClientService = MockRepository.Create<IEdgeDeviceConfigurationsClientService>();


            _ = Services.AddSingleton(this.mockEdgeDeviceConfigurationsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });
        }

        [Test]
        public void ConfigsPageMustLoadConfigurations()
        {
            // Arrange
            var configurations = new List<ConfigListItem>
            {
                new(),
                new()
            };

            _ = this.mockEdgeDeviceConfigurationsClientService
                .Setup(service => service.GetDeviceConfigurations())
                .ReturnsAsync(configurations);

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ConfigsPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfigurations()
        {
            // Arrange
            _ = this.mockEdgeDeviceConfigurationsClientService
                .Setup(service => service.GetDeviceConfigurations())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickToItemShouldRedirectToConfigurationDetailsPage()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();

            var configurations = new List<ConfigListItem>
            {
                new()
                {
                    ConfigurationID = configurationId
                }
            };

            _ = this.mockEdgeDeviceConfigurationsClientService
                .Setup(service => service.GetDeviceConfigurations())
                .ReturnsAsync(configurations);

            var cut = RenderComponent<Configs>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith($"/edge/configurations/{configurationId}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
