// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Devices
{
    using System.Collections.Generic;
    using AngleSharp.Dom;
    using AutoFixture;
    using Bunit;
    using FluentAssertions;
    using IoTHub.Portal.Client.Components.Devices;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Shared;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class DeviceToDuplicateSelectorTests : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();
        }

        [Test]
        public void DeviceToDuplicateSelectorShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<DeviceToDuplicateSelector>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#search-device").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void TypingOnMudAutocompleteShouldTriggerSearch()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var url = $"api/devices?pageSize=10&searchText={query}";
            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<DeviceListItem>()
                {
                    Items = new List<DeviceListItem>
                    {
                        new()
                        {
                            DeviceID = Fixture.Create<string>()
                        }
                    }
                });

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceToDuplicateSelector>();
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SelectDeviceShouldDuplicateDeviceAndItsModel()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var expectedDeviceModel = new DeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedDevice = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                ModelId = expectedDeviceModel.ModelId
            };

            var expectedDeviceItem = new DeviceListItem
            {
                DeviceID = expectedDevice.DeviceID,
            };

            var url = $"api/devices?pageSize=10&searchText={query}";
            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<DeviceListItem>()
                {
                    Items = new List<DeviceListItem>
                    {
                        expectedDeviceItem
                    }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetDevice(expectedDevice.DeviceID))
                .ReturnsAsync(expectedDevice);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(expectedDevice.ModelId))
                .ReturnsAsync(expectedDeviceModel);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceToDuplicateSelector>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            // Act
            var item = popoverProvider.Find("div.mud-list-item");
            item.Click();

            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeFalse());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SelectDeviceShouldProcessProblemDetailsExceptionWhenGettingDeviceDetails()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var expectedDeviceModel = new DeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedDevice = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                ModelId = expectedDeviceModel.ModelId
            };

            var expectedDeviceItem = new DeviceListItem
            {
                DeviceID = expectedDevice.DeviceID,
            };

            var url = $"api/devices?pageSize=10&searchText={query}";
            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<DeviceListItem>()
                {
                    Items = new List<DeviceListItem>
                    {
                        expectedDeviceItem
                    }
                });

            _ = this.mockDeviceClientService.Setup(service => service.GetDevice(expectedDevice.DeviceID))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceToDuplicateSelector>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            // Act
            var item = popoverProvider.Find("div.mud-list-item");
            item.Click();

            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeFalse());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SelectLoraDeviceShouldDuplicateLoraDeviceAndItsLoraModel()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var expectedDeviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedDevice = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                ModelId = expectedDeviceModel.ModelId
            };

            var expectedDeviceItem = new DeviceListItem
            {
                DeviceID = expectedDevice.DeviceID,
                SupportLoRaFeatures = true
            };

            var url = $"api/devices?pageSize=10&searchText={query}";
            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<DeviceListItem>()
                {
                    Items = new List<DeviceListItem>
                    {
                        expectedDeviceItem
                    }
                });

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetDevice(expectedDevice.DeviceID))
                .ReturnsAsync(expectedDevice);

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service => service.GetDeviceModel(expectedDevice.ModelId))
                .ReturnsAsync(expectedDeviceModel);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<DeviceToDuplicateSelector>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            // Act
            var item = popoverProvider.Find("div.mud-list-item");
            item.Click();

            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeFalse());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void TypingOnMudAutocompleteShouldProcessProblemDetailsExceptionWhenTriggerSearch()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var url = $"api/devices?pageSize=10&searchText={query}";
            _ = this.mockDeviceClientService.Setup(service => service.GetDevices(url))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<DeviceToDuplicateSelector>();
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
