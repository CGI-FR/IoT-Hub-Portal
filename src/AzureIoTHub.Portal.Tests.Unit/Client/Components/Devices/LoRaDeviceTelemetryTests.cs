// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Components.Devices
{
    using System.Linq;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Components.Devices;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;

    [TestFixture]
    public class LoRaDeviceTelemetryTests : BlazorUnitTest
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
        public void OnAfterRenderAsync_TelemetryLoaded_CanvasIsRendered()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetDeviceTelemetry(deviceId))
                .ReturnsAsync(Fixture.CreateMany<LoRaDeviceTelemetryDto>(1).ToList());

            // Act
            var cut = RenderComponent<LoRaDeviceTelemetry>(ComponentParameter.CreateParameter("DeviceId", deviceId));
            _ = cut.WaitForElement("canvas");

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnAfterRenderAsync_ProblemDetailsExceptionIsThrown_CanvasIsNotRendered()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.GetDeviceTelemetry(deviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<LoRaDeviceTelemetry>(ComponentParameter.CreateParameter("DeviceId", deviceId));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => cut.FindAll("canvas").Count.Should().Be(0));
        }
    }
}
