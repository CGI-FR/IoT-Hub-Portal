// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Configurations
{
    using System;
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
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigDetailTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<IEdgeDeviceConfigurationsClientService> mockEdgeDeviceConfigurationsClientService;

        private readonly string mockConfigurationId = Guid.NewGuid().ToString();

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockEdgeDeviceConfigurationsClientService = MockRepository.Create<IEdgeDeviceConfigurationsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockEdgeDeviceConfigurationsClientService.Object);
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockEdgeDeviceConfigurationsClientService
                .Setup(service => service.GetDeviceConfiguration(this.mockConfigurationId))
                .ReturnsAsync(new ConfigListItem());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationId));

            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/edge/configurations"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ConfigDetailShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfiguration()
        {
            // Arrange
            _ = this.mockEdgeDeviceConfigurationsClientService
                .Setup(service => service.GetDeviceConfiguration(this.mockConfigurationId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });


            // Act
            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationId));

            // Assert
            _ = cut.Markup.Should().NotBeEmpty();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
