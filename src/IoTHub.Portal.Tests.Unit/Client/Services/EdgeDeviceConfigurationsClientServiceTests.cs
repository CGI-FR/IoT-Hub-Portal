// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Linq;
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
    public class EdgeDeviceConfigurationsClientServiceTests : BlazorUnitTest
    {
        private IEdgeDeviceConfigurationsClientService edgeDeviceConfigurationsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IEdgeDeviceConfigurationsClientService, EdgeDeviceConfigurationsClientService>();

            this.edgeDeviceConfigurationsClientService = Services.GetRequiredService<IEdgeDeviceConfigurationsClientService>();
        }

        [Test]
        public async Task GetDeviceConfigurationsShouldReturnDeviceConfigurations()
        {
            // Arrange
            var expectedConfigurations = Fixture.Build<ConfigListItem>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/edge/configurations")
                .RespondJson(expectedConfigurations);

            // Act
            var result = await this.edgeDeviceConfigurationsClientService.GetDeviceConfigurations();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConfigurations);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetDeviceConfigurationShouldReturnDeviceConfiguration()
        {
            // Arrange
            var expectedConfiguration = Fixture.Create<ConfigListItem>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/edge/configurations/{expectedConfiguration.ConfigurationID}")
                .RespondJson(expectedConfiguration);

            // Act
            var result = await this.edgeDeviceConfigurationsClientService.GetDeviceConfiguration(expectedConfiguration.ConfigurationID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConfiguration);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
