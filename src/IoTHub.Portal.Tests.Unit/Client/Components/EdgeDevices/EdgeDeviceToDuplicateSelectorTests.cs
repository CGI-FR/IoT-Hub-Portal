// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.EdgeDevices
{
    using System.Collections.Generic;
    using AngleSharp.Dom;
    using AutoFixture;
    using IoTHub.Portal.Client.Components.EdgeDevices;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Shared;
    using Shared.Models.v1._0;
    using UnitTests.Bases;

    [TestFixture]
    public class EdgeDeviceToDuplicateSelectorTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;
        private Mock<IEdgeModelClientService> mockEdgeDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();
            this.mockEdgeDeviceModelsClientService = MockRepository.Create<IEdgeModelClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockEdgeDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);

            _ = Services.AddSingleton<IEdgeDeviceLayoutService, EdgeDeviceLayoutService>();
        }

        [Test]
        public void EdgeDeviceToDuplicateSelectorShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<EdgeDeviceToDuplicateSelector>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#search-device").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void TypingOnMudAutocompleteShouldTriggerSearch()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var url = $"api/edge/devices?pageSize=10&searchText={query}";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>()
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new()
                        {
                            DeviceId = Fixture.Create<string>()
                        }
                    }
                });

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceToDuplicateSelector>();
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeListItem>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SelectEdgeDeviceShouldDuplicateDeviceAndItsModel()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var expectedDeviceModel = new IoTEdgeModel
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedDevice = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>(),
                ModelId = expectedDeviceModel.ModelId
            };

            var expectedDeviceItem = new IoTEdgeListItem
            {
                DeviceId = expectedDevice.DeviceId,
            };

            var url = $"api/edge/devices?pageSize=10&searchText={query}";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>()
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        expectedDeviceItem
                    }
                });

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(expectedDevice.DeviceId))
                .ReturnsAsync(expectedDevice);

            _ = this.mockEdgeDeviceModelsClientService.Setup(service => service.GetIoTEdgeModel(expectedDevice.ModelId))
                .ReturnsAsync(expectedDeviceModel);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceToDuplicateSelector>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeListItem>>();
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
        public void SelectEdgeDeviceShouldProcessProblemDetailsExceptionWhenGettingDeviceDetails()
        {
            // Arrange
            var query = Fixture.Create<string>();

            var expectedDeviceModel = new IoTEdgeModel
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedDevice = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>(),
                ModelId = expectedDeviceModel.ModelId
            };

            var expectedDeviceItem = new IoTEdgeListItem
            {
                DeviceId = expectedDevice.DeviceId,
            };

            var url = $"api/edge/devices?pageSize=10&searchText={query}";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(url))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>()
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        expectedDeviceItem
                    }
                });

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(expectedDevice.DeviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceToDuplicateSelector>();

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeListItem>>();
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

            var url = $"api/edge/devices?pageSize=10&searchText={query}";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(url))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<EdgeDeviceToDuplicateSelector>();
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeListItem>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
