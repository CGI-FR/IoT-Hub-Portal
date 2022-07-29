// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System.Collections.Generic;
    using AngleSharp.Dom;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.Devices;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

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

            var cut = RenderComponent<DeviceToDuplicateSelector>();
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<DeviceListItem>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);


            // Assert
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
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
