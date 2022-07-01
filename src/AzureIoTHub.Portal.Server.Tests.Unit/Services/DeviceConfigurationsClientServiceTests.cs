// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Linq;
    using System.Net;
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
