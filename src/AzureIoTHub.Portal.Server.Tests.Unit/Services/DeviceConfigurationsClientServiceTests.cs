// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
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

        [Test]
        public async Task DeleteDeviceConfigurationsMustThrowProblemDetailsExceptionWhenErrorOccurs()
        {
            // Arrange
            var configurationId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/device-configurations/{configurationId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var act = () => this.deviceConfigurationsClientService.DeleteDeviceConfiguration(configurationId);

            // Assert
            _ = await act.Should().ThrowAsync<ProblemDetailsException>();
        }
    }
}
