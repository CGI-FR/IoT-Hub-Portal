// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
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
    public class CreateDevicePageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;

        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public async Task ClickOnSaveShouldPostDeviceDetailsAsync()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceClientService.Setup(service => service.CreateDevice(It.Is<DeviceDetails>(details => expectedDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceClientService
                .Setup(service => service.SetDeviceProperties(expectedDeviceDetails.DeviceID, It.IsAny<IList<DevicePropertyValue>>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
        }

        [Test]
        public async Task DeviceShouldNotBeCreatedWhenModelIsNotValid()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(string.Empty);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(string.Empty);
            await cut.Instance.ChangeModel(mockDeviceModel);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task SaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingDevice()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceClientService.Setup(service => service.CreateDevice(It.Is<DeviceDetails>(details => expectedDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            // Act
            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ChangeModelShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingModelProperties()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                    .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateDevicePage>();

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnSaveAndAddNewShouldCreateDeviceAndResetCreateDevicePage()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceClientService.Setup(service => service.CreateDevice(It.Is<DeviceDetails>(details => expectedDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceClientService
                .Setup(service => service.SetDeviceProperties(expectedDeviceDetails.DeviceID, It.IsAny<IList<DevicePropertyValue>>()))
                .Returns(Task.CompletedTask);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Save and New
            items[1].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceDetails.DeviceName)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceDetails.DeviceID)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("/devices"));
        }

        [Test]
        public async Task ClickOnSaveAndDuplicateShouldCreateDeviceAndDuplicateDeviceDetailsInCreateDevicePage()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceClientService.Setup(service => service.CreateDevice(It.Is<DeviceDetails>(details => expectedDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(new List<DeviceModelDto>
                {
                    mockDeviceModel
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModelModelProperties(mockDeviceModel.ModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceClientService
                .Setup(service => service.SetDeviceProperties(expectedDeviceDetails.DeviceID, It.IsAny<IList<DevicePropertyValue>>()))
                .Returns(Task.CompletedTask);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Save and Duplicate
            items[2].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceDetails.DeviceName)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceDetails.DeviceID)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("/devices"));
        }
    }
}
