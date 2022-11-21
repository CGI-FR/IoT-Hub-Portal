// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.DevicesModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Client.Services;
    using Models;
    using Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Mocks;

    [TestFixture]
    public class CreateDeviceModelPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
        }

        [Test]
        public void ClickOnSaveShouldPostNonLoRaDeviceModelData()
        {
            // Arrange
            var modelName = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.CreateDeviceModel(It.Is<DeviceModelDto>(model =>
                        modelName.Equals(model.Name, StringComparison.Ordinal) && description.Equals(model.Description, StringComparison.Ordinal) && !model.SupportLoRaFeatures)))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), new List<DeviceProperty>()))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change(modelName);
            cut.WaitForElement($"#{nameof(DeviceModelDto.Description)}").Change(description);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingDeviceModel()
        {
            // Arrange
            var modelName = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.CreateDeviceModel(It.Is<DeviceModelDto>(model =>
                        modelName.Equals(model.Name, StringComparison.Ordinal) && description.Equals(model.Description, StringComparison.Ordinal) && !model.SupportLoRaFeatures)))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change(modelName);
            cut.WaitForElement($"#{nameof(DeviceModelDto.Description)}").Change(description);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().NotEndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddPropertyShouldAddNewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.CreateDeviceModel(It.IsAny<DeviceModelDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), It.Is<List<DeviceProperty>>(properties => properties.Count.Equals(1))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addPropertyButton");

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change(Guid.NewGuid().ToString());
            cut.WaitForElement($"#{nameof(DeviceModelDto.Description)}").Change(Guid.NewGuid().ToString());

            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-{propertyName}";

            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(nameof(DevicePropertyType.Boolean));
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#deletePropertyButton").Count));

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRemovePropertyShouldRemoveTheProperty()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.CreateDeviceModel(It.IsAny<DeviceModelDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), It.Is<List<DeviceProperty>>(properties => properties.Count.Equals(0))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change(Guid.NewGuid().ToString());
            cut.WaitForElement($"#{nameof(DeviceModelDto.Description)}").Change(Guid.NewGuid().ToString());

            var addPropertyButton = cut.WaitForElement("#addPropertyButton");
            addPropertyButton.Click();

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#deletePropertyButton").Count));

            var removePropertyButton = cut.WaitForElement("#deletePropertyButton");
            removePropertyButton.Click();

            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll("#deletePropertyButton").Count));

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsDisabledModelDetailsShouldNotDisplayLoRaWANSwitch()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#form"));
            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll("#SupportLoRaFeatures").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANSwitch()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#form"));
            cut.WaitForAssertion(() => cut.Find("#SupportLoRaFeatures"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANTab()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#form"));
            cut.WaitForElement("#SupportLoRaFeatures").Change(true);

            cut.WaitForState(() => cut.FindAll(".mud-tabs .mud-tab").Count == 2);

            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");

            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldPostLoRaDeviceModelData()
        {
            // Arrange
            var modelName = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.CreateDeviceModel(It.Is<LoRaDeviceModelDto>(model =>
                        modelName.Equals(model.Name, StringComparison.Ordinal) && description.Equals(model.Description, StringComparison.Ordinal) && model.SupportLoRaFeatures)))
                .Returns(Task.CompletedTask);

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelCommands(It.IsAny<string>(), new List<DeviceModelCommandDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement("#SupportLoRaFeatures").Change(true);
            cut.WaitForState(() => cut.FindAll(".mud-tabs .mud-tab").Count == 2);

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change(modelName);
            cut.WaitForElement($"#{nameof(DeviceModelDto.Description)}").Change(description);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
