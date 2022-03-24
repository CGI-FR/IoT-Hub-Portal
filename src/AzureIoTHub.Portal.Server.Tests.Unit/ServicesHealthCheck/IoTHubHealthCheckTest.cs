// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.ServicesHealthCheck
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.ServicesHealthCheck;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class IoTHubHealthCheckTest
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;
        private Mock<ServiceClient> mockServiceClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockServiceClient = this.mockRepository.Create<ServiceClient>();
        }

        private IoTHubHealthCheck CreateHealthCheck()
        {
            return new IoTHubHealthCheck(this.mockRegistryManager.Object, this.mockServiceClient.Object);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var healthService = CreateHealthCheck();

            var mockServiceStat = this.mockRepository.Create<ServiceStatistics>();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var healthCheckContext = new HealthCheckContext();
            var token = new CancellationToken();

            _ = mockQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ $0: 2}"
                });

            _ = this.mockServiceClient
                .Setup(c => c.GetServiceStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockServiceStat.Object);

            _ = this.mockRegistryManager
                .Setup(c => c.CreateQuery(It.Is<string>(x => x == "")))
                .Returns(mockQuery.Object);

            // Act
            var result = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
