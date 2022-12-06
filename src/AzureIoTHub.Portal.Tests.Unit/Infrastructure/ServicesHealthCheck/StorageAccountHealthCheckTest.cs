// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class StorageAccountHealthCheckTest
    {
        private MockRepository mockRepository;
        private Mock<BlobServiceClient> mockBlobServiceClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockBlobServiceClient = this.mockRepository.Create<BlobServiceClient>();
        }

        private StorageAccountHealthCheck CreateHealthService()
        {
            return new StorageAccountHealthCheck(this.mockBlobServiceClient.Object);
        }

        [Test]
        public async Task CheckHealthAsyncContainerExistReturnHealthy()
        {
            // Arrange
            var healthService = CreateHealthService();
            var healthRegistration = new HealthCheckRegistration(Guid.NewGuid().ToString(), healthService, HealthStatus.Healthy, new List<string>());

            var healthCheckContext = new HealthCheckContext()
            {
                Registration = healthRegistration
            };
            var token = new CancellationToken(canceled:false);

            var blobContainerClient = this.mockRepository.Create<BlobContainerClient>();
            var responseContainerExist = this.mockRepository.Create<Azure.Response<bool>>();
            var responseGetPropertiesAsync = this.mockRepository.Create<Azure.Response<BlobContainerProperties>>();

            _ = responseContainerExist
                .SetupGet(c => c.Value)
                .Returns(true);

            _ = this.mockBlobServiceClient
                .Setup(c => c.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClient.Object);

            _ = blobContainerClient
                .Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseContainerExist.Object);

            _ = blobContainerClient
                .Setup(c => c.GetPropertiesAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseGetPropertiesAsync.Object);

            // Act
            var result  = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [Test]
        public async Task CheckHealthAsyncContainerDoesNotExistReturnUnHealthy()
        {
            // Arrange
            var healthService = CreateHealthService();

            var healthRegistration = new HealthCheckRegistration(Guid.NewGuid().ToString(), healthService, HealthStatus.Unhealthy, new List<string>());

            var healthCheckContext = new HealthCheckContext()
            {
                Registration = healthRegistration
            };

            var token = new CancellationToken(canceled:false);

            var blobContainerClient = this.mockRepository.Create<BlobContainerClient>();
            var responseContainerExist = this.mockRepository.Create<Azure.Response<bool>>();
            var responseGetPropertiesAsync = this.mockRepository.Create<Azure.Response<BlobContainerProperties>>();

            _ = responseContainerExist
                .SetupGet(c => c.Value)
                .Returns(false);

            _ = this.mockBlobServiceClient
                .Setup(c => c.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClient.Object);

            _ = blobContainerClient
                .Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseContainerExist.Object);

            _ = blobContainerClient
                .Setup(c => c.GetPropertiesAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseGetPropertiesAsync.Object);

            // Act
            var result  = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }
    }
}
