// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Pages.Devices;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Client.Dialogs.Devices;
    using UnitTests.Bases;
    using Bunit;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using AutoFixture;
    using System.Linq;
    using FluentAssertions;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class LoRaDeviceTelemetryDialogTests : BlazorUnitTest
    {
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();
        }

        [Test]
        public async Task Show_TelemetryLoaded_CanvasIsRendered()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetDeviceTelemetry(deviceId))
                .ReturnsAsync(Fixture.CreateMany<LoRaDeviceTelemetryDto>(1).ToList());

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<LoRaDeviceTelemetryDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("canvas");

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task Show_CancelDialog_DialogIsCancelled()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetDeviceTelemetry(deviceId))
                .ReturnsAsync(Fixture.CreateMany<LoRaDeviceTelemetryDto>(1).ToList());

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<LoRaDeviceTelemetryDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("canvas");
            var deleteBtn = cut.Find("#cancel-telemetry-dialog");
            deleteBtn.Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
