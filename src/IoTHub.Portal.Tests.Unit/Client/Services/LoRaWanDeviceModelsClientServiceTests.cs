// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using Shared.Models.v1._0.LoRaWAN;

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

            var expectedDeviceModel = new LoRaDeviceModelDto
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
        public async Task CreateDeviceModelShouldCreateDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Post, "/api/lorawan/models")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceModelDto>>();
                    var body = m.Content as ObjectContent<LoRaDeviceModelDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedDeviceModel);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceModelsClientService.CreateDeviceModel(expectedDeviceModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceModelShouldUpdateDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/lorawan/models/{expectedDeviceModel.ModelId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceModelDto>>();
                    var body = m.Content as ObjectContent<LoRaDeviceModelDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedDeviceModel);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceModelsClientService.UpdateDeviceModel(expectedDeviceModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SetDeviceModelCommandsShouldSetDeviceModelCommands()
        {
            // Arrange
            var deviceModelId = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedCommands = Fixture.Build<DeviceModelCommandDto>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/lorawan/models/{deviceModelId.ModelId}/commands")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IList<DeviceModelCommandDto>>>();
                    var body = m.Content as ObjectContent<IList<DeviceModelCommandDto>>;
                    _ = body.Value.Should().BeEquivalentTo(expectedCommands);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceModelsClientService.SetDeviceModelCommands(deviceModelId.ModelId, expectedCommands);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceModelCommandsShouldReturnCommands()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var expectedCommands = Fixture.CreateMany<DeviceModelCommandDto>().ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModelId}/commands")
                .RespondJson(expectedCommands);

            // Act
            var result = await this.loRaWanDeviceModelsClientService.GetDeviceModelCommands(deviceModelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedCommands);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetAvatarUrlShouldReturnAvatarUrl()
        {
            // Arrange
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>(),
                ImageUrl = Fixture.Create<Uri>()
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModel.ModelId}/avatar")
                .RespondJson(deviceModel.ImageUrl.ToString());

            // Act
            var result = await this.loRaWanDeviceModelsClientService.GetAvatarUrl(deviceModel.ModelId);

            // Assert
            _ = result.Should().Contain(deviceModel.ImageUrl.ToString());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangeAvatarPropertiesShouldChangeAvatar()
        {
            // Arrange
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            using var content = new MultipartFormDataContent();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/lorawan/models/{deviceModel.ModelId}/avatar")
                .With(m =>
                {
                    _ = m.Content.Should().BeEquivalentTo(content);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanDeviceModelsClientService.ChangeAvatar(deviceModel.ModelId, content);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
