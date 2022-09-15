// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceModelsClientServiceTests : BlazorUnitTest
    {
        private IDeviceModelsClientService deviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IDeviceModelsClientService, DeviceModelsClientService>();

            this.deviceModelsClientService = Services.GetRequiredService<IDeviceModelsClientService>();
        }

        [Test]
        public async Task GetDeviceModelsShouldReturnDeviceModels()
        {
            // Arrange
            var expectedDeviceModels = Fixture.Build<DeviceModelDto>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/models")
                .RespondJson(expectedDeviceModels);

            // Act
            var result = await this.deviceModelsClientService.GetDeviceModels();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModels);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceModelShouldReturnDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{expectedDeviceModel.ModelId}")
                .RespondJson(expectedDeviceModel);

            // Act
            var result = await this.deviceModelsClientService.GetDeviceModel(expectedDeviceModel.ModelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModel);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateDeviceModelShouldCreateDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/models")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceModelDto>>();
                    var body = m.Content as ObjectContent<DeviceModelDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedDeviceModel);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceModelsClientService.CreateDeviceModel(expectedDeviceModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceModelShouldUpdateDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/models/{expectedDeviceModel.ModelId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceModelDto>>();
                    var body = m.Content as ObjectContent<DeviceModelDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedDeviceModel);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceModelsClientService.UpdateDeviceModel(expectedDeviceModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteDeviceModelShouldDeleteDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/models/{expectedDeviceModel.ModelId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.deviceModelsClientService.DeleteDeviceModel(expectedDeviceModel.ModelId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceModelModelPropertiesShouldReturnDeviceModelModelProperties()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();
            var expectedDeviceModelProperties = Fixture.Build<DeviceProperty>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{deviceModel.ModelId}/properties")
                .RespondJson(expectedDeviceModelProperties);

            // Act
            var result = await this.deviceModelsClientService.GetDeviceModelModelProperties(deviceModel.ModelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModelProperties);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SetDeviceModelModelPropertiesShouldSetDeviceModelModelProperties()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();
            var expectedDeviceModelProperties = Fixture.Build<DeviceProperty>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{deviceModel.ModelId}/properties")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IList<DeviceProperty>>>();
                    var body = m.Content as ObjectContent<IList<DeviceProperty>>;
                    _ = body.Value.Should().BeEquivalentTo(expectedDeviceModelProperties);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceModelsClientService.SetDeviceModelModelProperties(deviceModel.ModelId, expectedDeviceModelProperties);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetAvatarUrlShouldReturnAvatarUrl()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{deviceModel.ModelId}/avatar")
                .RespondJson(deviceModel.ImageUrl.ToString());

            // Act
            var result = await this.deviceModelsClientService.GetAvatarUrl(deviceModel.ModelId);

            // Assert
            _ = result.Should().Contain(deviceModel.ImageUrl.ToString());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangeAvatarPropertiesShouldChangeAvatar()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();
            using var content = new MultipartFormDataContent();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/models/{deviceModel.ModelId}/avatar")
                .With(m =>
                {
                    _ = m.Content.Should().BeEquivalentTo(content);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceModelsClientService.ChangeAvatar(deviceModel.ModelId, content);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
