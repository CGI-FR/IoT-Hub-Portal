// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

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

            _ = Services.AddSingleton<IEdgeDeviceLayoutService, EdgeDeviceLayoutService>();

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public async Task SaveShouldCreateEdgeDeviceAndRedirectToEdgeDeviceList()
        {
            // Arrange
            var edgeModel = new IoTEdgeModelListItem(){ Name = "model01", ModelId = "model01"};

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>() { edgeModel });

            _ = this.mockDeviceTagSettingsClientService.Setup(x => x.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto(){ Name = "tag01", Required = true}
                });

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.CreateDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns(value: null);

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
            var edgeModel = new IoTEdgeModelListItem(){ Name = "model01", ModelId = "model01"};

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>() { edgeModel });

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
            _ = this.mockIEdgeModelClientService.Setup(x => x.GetIoTEdgeModelList())
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
            var mockEdgeDeviceModel = new IoTEdgeModelListItem
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

            _ = this.mockIEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>
                {
                    mockEdgeDeviceModel
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

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns(value: null);

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
            var mockEdgeDeviceModel = new IoTEdgeModelListItem
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

            _ = this.mockIEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>
                {
                    mockEdgeDeviceModel
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

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns(value: null);

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
    }
}
