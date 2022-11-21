// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using AutoFixture;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using System.Linq;

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
            await cut.InvokeAsync(() => service?.Show<LoRaDeviceTelemetryDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("canvas");

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
