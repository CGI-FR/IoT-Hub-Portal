// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaManagementKeyFacadeHealthCheckTest
    {
        private MockRepository mockRepository;
        private Mock<ILoraDeviceMethodManager> mockoraDeviceMethodManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockoraDeviceMethodManager = this.mockRepository.Create<ILoraDeviceMethodManager>();
        }

        private LoRaManagementKeyFacadeHealthCheck CreateService()
        {
            return new LoRaManagementKeyFacadeHealthCheck(this.mockoraDeviceMethodManager.Object);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderTestReturnHealthy()
        {
            // Arrange
            var healthService = CreateService();

            var healthCheckContext = new HealthCheckContext();
            var token = new CancellationToken(canceled:false);

            var mockHttpMessage = this.mockRepository.Create<HttpResponseMessage>(System.Net.HttpStatusCode.OK);

            _ = this.mockoraDeviceMethodManager
                .Setup(c => c.CheckAzureFunctionReturn(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockHttpMessage.Object);

            // Act
            var result = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderTestReturnUnhealthy()
        {
            // Arrange
            var healthService = CreateService();

            var healthRegistration = new HealthCheckRegistration(Guid.NewGuid().ToString(), healthService, HealthStatus.Unhealthy, new List<string>());
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = healthRegistration
            };
            var token = new CancellationToken(canceled:false);

            var mockHttpMessage = this.mockRepository.Create<HttpResponseMessage>(System.Net.HttpStatusCode.BadRequest);

            _ = this.mockoraDeviceMethodManager
                .Setup(c => c.CheckAzureFunctionReturn(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockHttpMessage.Object);

            // Act
            var result = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }
    }
}
