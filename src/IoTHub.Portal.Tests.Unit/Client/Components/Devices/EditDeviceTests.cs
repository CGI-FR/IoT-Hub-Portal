// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using IoTHub.Portal.Client.Components.Devices;
    using IoTHub.Portal.Client.Dialogs.Devices;
    using IoTHub.Portal.Client.Enums;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.Devices;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Constants;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using IoTHub.Portal.Tests.Unit.UnitTests.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Shared;
    using Shared.Models;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class EditDeviceTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        //private Mock<ISnackbar> mockSnackbarService;
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
            //this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            //_ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);
            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = CloudProviders.Azure });
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
                .ReturnsAsync(expectedDeviceDetails.DeviceID);

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

            var cut = RenderComponent<EditDevice>();
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

            var cut = RenderComponent<EditDevice>();
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EditDevice>();

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
            var cut = RenderComponent<EditDevice>();
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

            var cut = RenderComponent<EditDevice>();

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnSaveShouldCreateDeviceInEditDevice()
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
                .ReturnsAsync(expectedDeviceDetails.DeviceID);

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
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Create));
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Save and Duplicate
            items[0].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
        }

        [Test]
        public async Task ClickOnSaveShouldUpdateDeviceInEditDevice()
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

            _ = this.mockDeviceClientService.Setup(service => service.GetDevice(It.IsAny<string>()))
                .ReturnsAsync(new DeviceDetails());

            _ = this.mockDeviceClientService.Setup(service => service.GetDeviceProperties(It.IsAny<string>()))
                .ReturnsAsync(new List<DevicePropertyValue>());

            _ = this.mockDeviceModelsClientService
                .Setup(service => service.GetDeviceModel(It.IsAny<string>()))
                .ReturnsAsync(new DeviceModelDto());

            _ = this.mockDeviceClientService.Setup(service => service.UpdateDevice(It.Is<DeviceDetails>(details => expectedDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

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
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, expectedDeviceDetails.DeviceID));
            var saveButton = cut.WaitForElement("#saveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(2));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Save
            items[0].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
        }

        [Test]
        public async Task ClickOnSaveAndAddNewShouldCreateDeviceAndResetEditDevice()
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
                .ReturnsAsync(expectedDeviceDetails.DeviceID);

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
            var cut = RenderComponent<EditDevice>();
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
        public async Task ClickOnSaveAndDuplicateShouldCreateDeviceAndDuplicateDeviceDetailsInEditDevice()
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
                .ReturnsAsync(expectedDeviceDetails.DeviceID);

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
            var cut = RenderComponent<EditDevice>();
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

        [Test]
        public void SearchDeviceModelsInputExistingDeviceModelNameDeviceModelReturned()
        {
            // Arrange
            var deviceModels = Fixture.CreateMany<DeviceModelDto>(2).ToList();
            var expectedDeviceModel = deviceModels.First();

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.Is<DeviceModelFilter>(x => expectedDeviceModel.Name.Equals(x.SearchText, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = deviceModels.Where(x => expectedDeviceModel.Name.Equals(x.Name, StringComparison.Ordinal))
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var popoverProvider = RenderComponent<MudPopoverProvider>();

            var cut = RenderComponent<EditDevice>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IDeviceModel>>();

            // Act
            autocompleteComponent.Find("input").Input(expectedDeviceModel.Name);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-popover-open").Count.Should().Be(1));
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            var items = popoverProvider.FindComponents<MudListItem>().ToArray();
            _ = items.Length.Should().Be(1);
            _ = items.First().Markup.Should().Contain(expectedDeviceModel.Name);
            items.First().Find("div.mud-list-item").Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DisplayDeviceToDuplicateSelectorRenderCorrectly()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<EditDevice>();

            // Act
            cut.WaitForElement("#duplicate-device-switch").Change(true);

            // Assert
            cut.WaitForAssertion(() => cut.Find("label.mud-input-label").TextContent.Should().Be("Search a device to duplicate"));
        }

        [Test]
        public void DisplayValidationErrorMessageRenderCorrectly()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<EditDevice>();

            // Act
            cut.WaitForElement("#SaveButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Find("p.validation-error-message").TextContent.Should().Be("The Model is required."));
        }

        [Test]
        public async Task DisplayPropertiesRenderCorrectly()
        {
            // Arrange
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
                .ReturnsAsync(new List<DeviceProperty>
                {
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.Boolean,
                        IsWritable = true
                    },
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.Float,
                        IsWritable = true
                    },
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.Double,
                        IsWritable = true
                    },
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.Integer,
                        IsWritable = true
                    },
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.Long,
                        IsWritable = true
                    },
                    new()
                    {
                        Name = Guid.NewGuid().ToString(),
                        DisplayName = Guid.NewGuid().ToString(),
                        Order = 1,
                        PropertyType = DevicePropertyType.String,
                        IsWritable = true
                    }
                });

            var cut = RenderComponent<EditDevice>();

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#device-properties > .mud-grid-item").Count.Should().Be(6));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DisplayCreateLoraDeviceComponentRenderCorrectly()
        {
            // Arrange
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = true,
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockLoRaWanDeviceModelsClientService
                .Setup(service => service.GetDeviceModel(It.IsAny<string>()))
                .ReturnsAsync(new LoRaDeviceModelDto());

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetGatewayIdList())
                .ReturnsAsync(new LoRaGatewayIDList());

            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Create));

            // Act
            await cut.Instance.ChangeModel(mockDeviceModel);
            cut.WaitForElement("div.mud-tooltip-root:nth-child(2) > div.mud-tab").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Find("div.mud-tooltip-root:nth-child(2) > div.mud-tab").TextContent.Should().Be("LoRaWAN"));
        }

        [Test]
        public void ShouldLoadDeviceDetails()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(deviceId))
                .ReturnsAsync(new DeviceDetails() { ModelId = modelId });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(deviceId))
                .ReturnsAsync(new List<DevicePropertyValue>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(modelId))
                .ReturnsAsync(new DeviceModelDto());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var page = RenderComponent<DeviceDetailPage>(parameters => parameters.Add(p => p.DeviceID, deviceId));
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, deviceId));

            // Assert
            _ = page.WaitForElement("#returnButton");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ShouldLoadLoRaDeviceDetails()
        {

            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(deviceId))
                .ReturnsAsync(new DeviceDetails());

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(deviceId))
                .ReturnsAsync(new List<DevicePropertyValue>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(null))
                .ReturnsAsync(new DeviceModelDto());

            _ = this.mockLoRaWanDeviceClientService
                .Setup(service => service.GetDevice(deviceId))
                .ReturnsAsync(new LoRaDeviceDetails() { ModelId = modelId });

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModel(modelId))
                .ReturnsAsync(new LoRaDeviceModelDto());

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModelCommands(modelId))
                .ReturnsAsync(new List<DeviceModelCommandDto>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var page = RenderComponent<DeviceDetailPage>(parameters => parameters.Add(p => p.DeviceID, deviceId));
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, deviceId).Add(p => p.IsLoRa, true));

            // Assert
            _ = page.WaitForElement("#returnButton");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(deviceId))
                .ReturnsAsync(new DeviceDetails() { ModelId = modelId });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(deviceId))
                .ReturnsAsync(new List<DevicePropertyValue>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(modelId))
                .ReturnsAsync(new DeviceModelDto());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());


            // Act
            var page = RenderComponent<DeviceDetailPage>(parameters => parameters.Add(p => p.DeviceID, deviceId));
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, deviceId));
            var returnButton = page.WaitForElement("#returnButton");

            returnButton.Click();

            // Assert
            page.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        //[Test]
        //public void ClickOnSaveShouldPutDeviceDetails()
        //{
        //    var mockDeviceModel = new DeviceModelDto
        //    {
        //        ModelId = Guid.NewGuid().ToString(),
        //        Description = Guid.NewGuid().ToString(),
        //        SupportLoRaFeatures = false,
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    var mockTag = new DeviceTagDto
        //    {
        //        Label = Guid.NewGuid().ToString(),
        //        Name = Guid.NewGuid().ToString(),
        //        Required = false,
        //        Searchable = false
        //    };

        //    var mockDeviceDetails = new DeviceDetails
        //    {
        //        DeviceName = Guid.NewGuid().ToString(),
        //        ModelId = mockDeviceModel.ModelId,
        //        DeviceID = Guid.NewGuid().ToString(),
        //        Tags = new Dictionary<string, string>()
        //        {
        //            {mockTag.Name,Guid.NewGuid().ToString()}
        //        }
        //    };

        //    _ = this.mockDeviceClientService.Setup(service => service.UpdateDevice(It.Is<DeviceDetails>(details => mockDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
        //        .Returns(Task.CompletedTask);

        //    _ = this.mockDeviceClientService
        //        .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
        //        .ReturnsAsync(mockDeviceDetails);

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
        //        .ReturnsAsync(mockDeviceModel);

        //    _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
        //        .ReturnsAsync(new List<DeviceTagDto>
        //        {
        //            mockTag
        //        });

        //    _ = this.mockDeviceClientService
        //        .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
        //        .ReturnsAsync(new List<DevicePropertyValue>());

        //    _ = this.mockDeviceClientService
        //        .Setup(service => service.SetDeviceProperties(mockDeviceDetails.DeviceID, It.IsAny<IList<DevicePropertyValue>>()))
        //        .Returns(Task.CompletedTask);

        //    _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

        //    // Act
        //    var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));
        //    cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceModelDto.Name)}").InnerHtml.Should().NotBeEmpty());

        //    var saveButton = cut.WaitForElement("#saveButton");
        //    saveButton.Click();

        //    // Assert
        //    cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("devices", StringComparison.OrdinalIgnoreCase), 3.Seconds());
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        [Test]
        public void SaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdatingDevice()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTagDto
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };

            _ = this.mockDeviceClientService.Setup(service => service.UpdateDevice(It.Is<DeviceDetails>(details => mockDeviceDetails.DeviceID.Equals(details.DeviceID, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
                .ReturnsAsync(mockDeviceDetails);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
                .ReturnsAsync(mockDeviceModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
                .ReturnsAsync(new List<DevicePropertyValue>());

            // Act
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));


            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        //[Test]
        //public void ClickOnSaveShouldDisplaySnackbarIfValidationError()
        //{
        //    var mockDeviceModel = new DeviceModelDto
        //    {
        //        ModelId = Guid.NewGuid().ToString(),
        //        Description = Guid.NewGuid().ToString(),
        //        SupportLoRaFeatures = false,
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    var mockTag = new DeviceTagDto
        //    {
        //        Label = Guid.NewGuid().ToString(),
        //        Name = Guid.NewGuid().ToString(),
        //        Required = false,
        //        Searchable = false
        //    };

        //    var mockDeviceDetails = new DeviceDetails
        //    {
        //        DeviceName = Guid.NewGuid().ToString(),
        //        ModelId = mockDeviceModel.ModelId,
        //        DeviceID = Guid.NewGuid().ToString(),
        //        Tags = new Dictionary<string, string>()
        //        {
        //            {mockTag.Name,Guid.NewGuid().ToString()}
        //        }
        //    };

        //    _ = this.mockDeviceClientService
        //        .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
        //        .ReturnsAsync(mockDeviceDetails);

        //    _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
        //        .ReturnsAsync(mockDeviceModel);

        //    _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
        //        .ReturnsAsync(new List<DeviceTagDto>
        //        {
        //            mockTag
        //        });

        //    _ = this.mockDeviceClientService
        //        .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
        //        .ReturnsAsync(new List<DevicePropertyValue>());

        //    _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

        //    // Act
        //    var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));

        //    cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change("");
        //    var saveButton = cut.WaitForElement("#saveButton");
        //    saveButton.Click();

        //    // Assert
        //    cut.WaitForAssertion(() => MockRepository.VerifyAll());
        //}

        [Test]
        public void ClickOnConnectShouldDisplayDeviceCredentials()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTagDto
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
                .ReturnsAsync(mockDeviceDetails);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
                .ReturnsAsync(mockDeviceModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
                .ReturnsAsync(new List<DevicePropertyValue>());

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ConnectionStringDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));

            var connectButton = cut.WaitForElement("#connectButton");
            connectButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTagDto
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
                .ReturnsAsync(mockDeviceDetails);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
                .ReturnsAsync(mockDeviceModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
                .ReturnsAsync(new List<DevicePropertyValue>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDevicePage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTagDto
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
                .ReturnsAsync(mockDeviceDetails);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
                .ReturnsAsync(mockDeviceModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
                .ReturnsAsync(new List<DevicePropertyValue>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDevicePage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert            
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDuplicateShouldDuplicateDeviceDetailAndRedirectToCreateDevicePage()
        {
            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTagDto
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(mockDeviceDetails.DeviceID))
                .ReturnsAsync(mockDeviceDetails);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(mockDeviceDetails.ModelId))
                .ReturnsAsync(mockDeviceModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(mockDeviceDetails.DeviceID))
                .ReturnsAsync(new List<DevicePropertyValue>());

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EditDevice>(parameters => parameters.Add(p => p.context, CreateEditMode.Edit).Add(p => p.DeviceID, mockDeviceDetails.DeviceID));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceModelDto.Name)}").InnerHtml.Should().NotBeEmpty());

            var saveButton = cut.WaitForElement("#saveButton");

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(2));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Duplicate
            items[1].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices/new"));
        }
    }
}
