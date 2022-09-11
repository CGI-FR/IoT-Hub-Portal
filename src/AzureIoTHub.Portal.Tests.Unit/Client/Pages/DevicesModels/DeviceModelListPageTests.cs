// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.DevicesModels
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
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
    public class DeviceModelListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
        }

        [Test]
        public void WhenLoraFeatureDisableClickToItemShouldRedirectToDeviceDetailsPage()
        {
            // Arrange
            var modelId = Fixture.Create<string>();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = modelId, SupportLoRaFeatures = false } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith($"/device-models/{modelId}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureEnableClickToItemShouldRedirectToLoRaDeviceDetailsPage()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = modelId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/device-models/{modelId}?isLora=true"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceModelListPageRendersCorrectly()
        {
            // Arrange
            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] {
                    new DeviceModelDto { ModelId = Guid.NewGuid().ToString() },
                    new DeviceModelDto{  ModelId = Guid.NewGuid().ToString() }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var grid = cut.WaitForElement("div.mud-grid");

            Assert.IsNotNull(cut.Markup);
            Assert.IsNotNull(grid.InnerHtml);
            cut.WaitForAssertion(() => Assert.AreEqual("Device Models", cut.Find(".mud-typography-h6").TextContent));
            cut.WaitForAssertion(() => Assert.AreEqual(3, cut.FindAll("tr").Count));
            cut.WaitForAssertion(() => Assert.IsNotNull(cut.Find(".mud-table-container")));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceModelClickShouldNavigateToNewDeviceModelPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForElement("#addDeviceModelButton").Click();
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("device-models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LoadDeviceModelsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.WaitForElement("#tableRefreshButton").Click();
                await Task.Delay(0);
            }

            // Assert
            cut.WaitForAssertion(() => this.mockDeviceModelsClientService.Verify(service => service.GetDeviceModels(), Times.Exactly(4)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReloadDeviceModelIfConfirmed()
        {
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new[] { new DeviceModelDto { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert            
            cut.WaitForAssertion(() => this.mockDeviceModelsClientService.Verify(service => service.GetDeviceModels(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
