// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Azure.Data.Tables.Models;
    using AzureIoTHub.Portal.Infrastructure.Factories;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TableStorageHealthCheckTest
    {
        private MockRepository mockRepository;
        private Mock<ITableClientFactory> mockTableClientFactory;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
        }

        private TableStorageHealthCheck CreateHealthService()
        {
            return new TableStorageHealthCheck(this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderReturnHealthy()
        {
            // Arrange
            var healthService = CreateHealthService();

            var healthCheckContext = new HealthCheckContext();
            var token = new CancellationToken();

            var mockTable = this.mockRepository.Create<TableClient>();

            var responseCreateIfNotExist = this.mockRepository.Create<Response<TableItem>>();
            var responseAddentity = this.mockRepository.Create<Response>();

            _ = this.mockTableClientFactory
                .Setup(c => c.GetTemplatesHealthCheck())
                .Returns(mockTable.Object);

            _ = mockTable
                .Setup(c => c.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseCreateIfNotExist.Object);

            _ = mockTable
                .Setup(c => c.AddEntityAsync(It.IsAny<TableEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseAddentity.Object);

            _ = mockTable
                .Setup(c => c.DeleteEntityAsync(
                It.Is<string>(_ => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseAddentity.Object);

            _ = mockTable
                .Setup(c => c.DeleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseAddentity.Object);

            // Act
            var result  = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderReturnUnhealthy()
        {
            // Arrange
            var healthService = CreateHealthService();

            var healthRegistration = new HealthCheckRegistration(Guid.NewGuid().ToString(), healthService, HealthStatus.Unhealthy, new List<string>());
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = healthRegistration
            };
            var token = new CancellationToken();

            var mockTable = this.mockRepository.Create<TableClient>();

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockTableClientFactory
                .Setup(c => c.GetTemplatesHealthCheck())
                .Returns(mockTable.Object);

            _ = mockTable
                .Setup(c => c.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            _ = mockTable
                .Setup(c => c.AddEntityAsync(It.IsAny<TableEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = mockTable
                .Setup(c => c.DeleteEntityAsync(
                It.Is<string>(_ => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = mockTable
                .Setup(c => c.DeleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result  = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }
    }
}
