// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
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
    using Shared.Models.v1._0;

    [TestFixture]
    public class DeviceConfigurationsClientServiceTests : BlazorUnitTest
    {
        private IDeviceConfigurationsClientService deviceConfigurationsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IDeviceConfigurationsClientService, DeviceConfigurationsClientService>();

            this.deviceConfigurationsClientService = Services.GetRequiredService<IDeviceConfigurationsClientService>();
        }

        [Test]
        public async Task GetDeviceConfigurationsShouldReturnDeviceConfigurations()
        {
            // Arrange
            var expectedConfigurations = Fixture.Build<ConfigListItem>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/device-configurations")
                .RespondJson(expectedConfigurations);

            // Act
            var result = await this.deviceConfigurationsClientService.GetDeviceConfigurations();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConfigurations);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceConfigurationShouldReturnDeviceConfiguration()
        {
            // Arrange
            var expectedConfiguration = Fixture.Create<DeviceConfig>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/device-configurations/{expectedConfiguration.ConfigurationId}")
                .RespondJson(expectedConfiguration);

            // Act
            var result = await this.deviceConfigurationsClientService.GetDeviceConfiguration(expectedConfiguration.ConfigurationId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConfiguration);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceConfigurationMetricsShouldReturnDeviceConfigurationMetrics()
        {
            // Arrange
            var configuration = Fixture.Create<DeviceConfig>();
            var expectedConfigurationMetrics = Fixture.Create<ConfigurationMetrics>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/device-configurations/{configuration.ConfigurationId}/metrics")
                .RespondJson(expectedConfigurationMetrics);

            // Act
            var result = await this.deviceConfigurationsClientService.GetDeviceConfigurationMetrics(configuration.ConfigurationId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConfigurationMetrics);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateDeviceConfigurationShouldCreateDeviceConfiguration()
        {
            // Arrange
            var configuration = Fixture.Create<DeviceConfig>();

            _ = MockHttpClient.When(HttpMethod.Post,
                    "/api/device-configurations")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceConfig>>();
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceConfigurationsClientService.CreateDeviceConfiguration(configuration);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateDeviceConfigurationShouldUpdateDeviceConfiguration()
        {
            // Arrange
            var configuration = Fixture.Create<DeviceConfig>();

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/device-configurations/{configuration.ConfigurationId}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<DeviceConfig>>();
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.deviceConfigurationsClientService.UpdateDeviceConfiguration(configuration);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteDeviceConfigurationsShouldDeleteDeviceConfiguration()
        {
            // Arrange
            var configurationId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/device-configurations/{configurationId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.deviceConfigurationsClientService.DeleteDeviceConfiguration(configurationId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
