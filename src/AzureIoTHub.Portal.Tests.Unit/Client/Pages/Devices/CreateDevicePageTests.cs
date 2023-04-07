// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Mocks;

    [TestFixture]
    public class CreateDevicePageTests : BlazorUnitTest
    {
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
        }

        [Test]
        public void CreateDevicePageShouldRenderCorrectly()
        {
            // Arrange
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

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#SaveButton").TextContent.Should().Be("Save"));
        }
    }
}
