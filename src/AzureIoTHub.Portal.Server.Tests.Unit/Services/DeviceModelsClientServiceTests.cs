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
            var expectedDeviceModels = Fixture.Build<DeviceModel>().CreateMany(3).ToList();

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
            var expectedDeviceModel = Fixture.Create<DeviceModel>();

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
        public async Task GetDeviceModelModelPropertiesShouldReturnDeviceModelModelProperties()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
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
    }
}
