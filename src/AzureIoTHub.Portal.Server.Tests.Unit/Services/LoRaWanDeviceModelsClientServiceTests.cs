// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        public async Task CreateDeviceModelShouldCreateDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new LoRaDeviceModel
            {
                ModelId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Post, "/api/lorawan/models")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceModel>>();
                    var body = m.Content as ObjectContent<LoRaDeviceModel>;
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
            var expectedDeviceModel = new LoRaDeviceModel
            {
                ModelId = Fixture.Create<string>()
            };

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/lorawan/models/{expectedDeviceModel.ModelId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LoRaDeviceModel>>();
                    var body = m.Content as ObjectContent<LoRaDeviceModel>;
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
            var deviceModelId = new LoRaDeviceModel
            {
                ModelId = Fixture.Create<string>()
            };

            var expectedCommands = Fixture.Build<DeviceModelCommand>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModelId.ModelId}/properties")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IList<DeviceModelCommand>>>();
                    var body = m.Content as ObjectContent<IList<DeviceModelCommand>>;
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

        [Test]
        public async Task GetAvatarUrlShouldReturnAvatarUrl()
        {
            // Arrange
            var deviceModel = new LoRaDeviceModel
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
            var deviceModel = new LoRaDeviceModel
            {
                ModelId = Fixture.Create<string>()
            };
            var content = new MultipartFormDataContent();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/models/{deviceModel.ModelId}/properties")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<MultipartFormDataContent>>();
                    var body = m.Content as ObjectContent<MultipartFormDataContent>;
                    _ = body.Value.Should().BeEquivalentTo(content);
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
