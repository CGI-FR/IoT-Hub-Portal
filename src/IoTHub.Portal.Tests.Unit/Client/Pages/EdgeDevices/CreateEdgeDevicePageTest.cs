// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.EdgeDevices;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Constants;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class CreateEdgeDevicePageTest : BlazorUnitTest
    {
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;
        private Mock<IEdgeModelClientService> mockIEdgeModelClientService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();
            this.mockIEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockIEdgeModelClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = CloudProviders.Azure });

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = CloudProviders.Azure });

            _ = Services.AddSingleton<IEdgeDeviceLayoutService, EdgeDeviceLayoutService>();

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public async Task SaveShouldCreateEdgeDeviceAndRedirectToEdgeDeviceList()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel(){ Name = "model01", ModelId = "model01"};

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>() { edgeModel });

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModel(edgeModel.ModelId))
                .ReturnsAsync(edgeModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(x => x.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto(){ Name = "tag01", Required = true}
                });

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.CreateDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns(value: null);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateEdgeDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceName)}").Change("testName");
            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceId)}").Change(edgeDevice.DeviceId);
            cut.WaitForElement($"#tag01").Change("testTag01");
            await cut.Instance.ChangeModel(edgeModel);

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
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices"));
        }

        [Test]
        public async Task CreateEdgeDevicePageSaveShouldProcessProblemDetailsExceptionWhenIssueOccurs()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel(){ Name = "model01", ModelId = "model01"};

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>() { edgeModel });

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModel(edgeModel.ModelId))
                .ReturnsAsync(edgeModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(x => x.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto(){ Name = "tag01", Required = true}
                });

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.CreateDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateEdgeDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceName)}").Change("testName");
            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceId)}").Change(edgeDevice.DeviceId);
            cut.WaitForElement($"#tag01").Change("testTag01");
            await cut.Instance.ChangeModel(edgeModel);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            //cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices"));
        }

        [Test]
        public void WhenAnErrorOccurInOnInitializedAsyncShouldProcessProblemDetailsException()
        {
            // Arrange
            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList(null))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateEdgeDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnSaveAndDuplicateShouldCreateDeviceAndDuplicateEdgeDeviceDetailsInCreateEdgeDevicePage()
        {
            var mockEdgeDeviceModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                ImageUrl = Fixture.Create<Uri>(),
            };

            var expectedEdgeDevice = new IoTEdgeDevice
            {
                DeviceId = Guid.NewGuid().ToString(),
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockEdgeDeviceModel.ModelId
            };

            _ = this.mockEdgeDeviceClientService
                .Setup(service => service.CreateDevice(It.Is<IoTEdgeDevice>(device => expectedEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockIEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>
                {
                    mockEdgeDeviceModel
                });

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModel(mockEdgeDeviceModel.ModelId))
                .ReturnsAsync(mockEdgeDeviceModel);

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

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns(value: null);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateEdgeDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceName)}").Change("testName");
            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceId)}").Change(expectedEdgeDevice.DeviceId);
            await cut.Instance.ChangeModel(mockEdgeDeviceModel);

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
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("/edge/devices"));
        }

        [Test]
        public async Task ClickOnSaveAndAddNewShouldCreateEdgeDeviceAndResetCreateEdgeDevicePage()
        {
            var mockEdgeDeviceModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                ImageUrl = Fixture.Create<Uri>(),
            };

            var expectedEdgeDevice = new IoTEdgeDevice
            {
                DeviceId = Guid.NewGuid().ToString(),
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockEdgeDeviceModel.ModelId
            };

            _ = this.mockEdgeDeviceClientService
                .Setup(service => service.CreateDevice(It.Is<IoTEdgeDevice>(device => expectedEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockIEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>
                {
                    mockEdgeDeviceModel
                });

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModel(mockEdgeDeviceModel.ModelId))
                .ReturnsAsync(mockEdgeDeviceModel);

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

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns(value: null);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateEdgeDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceName)}").Change("testName");
            cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceId)}").Change(expectedEdgeDevice.DeviceId);
            await cut.Instance.ChangeModel(mockEdgeDeviceModel);

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Save and Duplicate
            items[1].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeDevice.DeviceName)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeDevice.DeviceId)}").TextContent.Should().BeEmpty());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("/edge/devices"));
        }

        [Test]
        public async Task ChangeEdgeModelShouldDisplayModelImage()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageUrl = Fixture.Create<Uri>()
            };

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>() { edgeModel });

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModel(edgeModel.ModelId))
                .ReturnsAsync(edgeModel);

            _ = this.mockDeviceTagSettingsClientService.Setup(x => x.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto(){ Name = "tag01", Required = true}
                });

            var cut = RenderComponent<CreateEdgeDevicePage>();

            var ModelImageElement = cut.WaitForElement($"#{nameof(IoTEdgeDevice.ImageUrl)}");

            await cut.Instance.ChangeModel(edgeModel);

            // Assert
            Assert.AreEqual(edgeModel.ImageUrl, ModelImageElement.Attributes["src"].Value);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchEdgeDeviceModels_InputExisingEdgeDeviceModelName_EdgeDeviceModelReturned()
        {
            // Arrange
            var edgeDeviceModels = Fixture.CreateMany<IoTEdgeModelListItem>(2).ToList();
            var expectedEdgeDeviceModel = edgeDeviceModels.First();

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList(null))
                .ReturnsAsync(edgeDeviceModels);

            _ = this.mockDeviceTagSettingsClientService.Setup(x => x.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto(){ Name = "tag01", Required = true}
                });

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateEdgeDevicePage>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeModel>>();

            // Act
            autocompleteComponent.Find("input").Input(expectedEdgeDeviceModel.Name);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-popover-open").Count.Should().Be(1));
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            var items = popoverProvider.FindComponents<MudListItem>().ToArray();
            _ = items.Length.Should().Be(1);
            _ = items.First().Markup.Should().Contain(expectedEdgeDeviceModel.Name);

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
