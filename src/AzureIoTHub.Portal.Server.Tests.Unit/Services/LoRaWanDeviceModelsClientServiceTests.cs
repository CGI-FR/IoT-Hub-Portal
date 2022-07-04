// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Linq;
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
    public class LoRaWanDeviceModelsClientServiceTests : BlazorUnitTest
    {
        private ILoRaWanDeviceModelsClientService loRaWanDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ILoRaWanDeviceModelsClientService, LoRaWanDeviceModelsClientService>();

            this.loRaWanDeviceModelsClientService = Services.GetRequiredService<ILoRaWanDeviceModelsClientService>();
        }

        [Test]
        public async Task GetDeviceModelShouldReturnDeviceModel()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var expectedDeviceModel = new LoRaDeviceModel
            {
                ModelId = deviceModelId
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModelId}")
                .RespondJson(expectedDeviceModel);

            // Act
            var result = await this.loRaWanDeviceModelsClientService.GetDeviceModel(deviceModelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModel);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceModelCommandsShouldReturnCommands()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var expectedCommands = Fixture.CreateMany<DeviceModelCommand>().ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModelId}/commands")
                .RespondJson(expectedCommands);

            // Act
            var result = await this.loRaWanDeviceModelsClientService.GetDeviceModelCommands(deviceModelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedCommands);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
