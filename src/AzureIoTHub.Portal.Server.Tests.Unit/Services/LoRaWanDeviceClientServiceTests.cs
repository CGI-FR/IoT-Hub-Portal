// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10.LoRaWAN;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class LoRaWanDeviceClientServiceTests : BlazorUnitTest
    {
        private ILoRaWanDeviceClientService loRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ILoRaWanDeviceClientService, LoRaWanDeviceClientService>();

            this.loRaWanDeviceClientService = Services.GetRequiredService<ILoRaWanDeviceClientService>();
        }

        [Test]
        public async Task GetDeviceShouldReturnDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            var expectedDevice = new LoRaDeviceDetails
            {
                DeviceID = deviceId
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/devices/{deviceId}")
                .RespondJson(expectedDevice);

            // Act
            var result = await this.loRaWanDeviceClientService.GetDevice(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateDeviceShouldCreateDevice()
        {
            // Arrange
            var device = new LoRaDeviceDetails();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/lorawan/devices")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceDetails>>();
                    var body = m.Content as ObjectContent<LoRaDeviceDetails>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceClientService.CreateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceShouldUpdateDevice()
        {
            // Arrange
            var device = new LoRaDeviceDetails();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/lorawan/devices")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceDetails>>();
                    var body = m.Content as ObjectContent<LoRaDeviceDetails>;
                    _ = body.Value.Should().BeEquivalentTo(device);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceClientService.UpdateDevice(device);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ExecuteCommandShouldExecuteCommand()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var commandId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/lorawan/devices/{deviceId}/_command/{commandId}")
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceClientService.ExecuteCommand(deviceId, commandId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
