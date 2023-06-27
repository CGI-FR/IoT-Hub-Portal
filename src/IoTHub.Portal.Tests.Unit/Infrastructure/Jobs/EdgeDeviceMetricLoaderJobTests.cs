// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using System.Threading;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Domain.Repositories;

    [TestFixture]
    public class EdgeDeviceMetricLoaderJobTests : IDisposable
    {
        private MockRepository mockRepository;
        private EdgeDeviceMetricLoaderJob edgeDeviceMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<EdgeDeviceMetricLoaderJob>> mockLogger;
        private Mock<IEdgeDeviceRepository> mockEdgeDeviceRepository;
        private Mock<IConfigService> mockConfigService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDeviceMetricLoaderJob>>();
            this.mockEdgeDeviceRepository = this.mockRepository.Create<IEdgeDeviceRepository>();
            this.mockConfigService = this.mockRepository.Create<IConfigService>();

            this.portalMetric = new PortalMetric();

            this.edgeDeviceMetricLoaderService =
                new EdgeDeviceMetricLoaderJob(this.mockLogger.Object, this.portalMetric, this.mockEdgeDeviceRepository.Object, this.mockConfigService.Object);
        }

        [Test]
        public void EdgeDeviceMetricLoaderServiceShouldLoadEdgeDeviceCountAndConnectedEdgeDeviceCountAndFailedDeploymentCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(10);

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(c => c.ConnectionState == "Connected", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(3);

            _ = this.mockConfigService.Setup(service => service.GetFailedDeploymentsCount()).ReturnsAsync(1);

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.edgeDeviceMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.EdgeDeviceCount.Should().Be(10);
            _ = this.portalMetric.ConnectedEdgeDeviceCount.Should().Be(3);
            _ = this.portalMetric.FailedDeploymentCount.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void EdgeDeviceMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingEdgeDeviceCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InternalServerErrorException(string.Empty));

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(c => c.ConnectionState == "Connected", It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            _ = this.mockConfigService.Setup(service => service.GetFailedDeploymentsCount()).ReturnsAsync(1);

            // Act
            _ = this.edgeDeviceMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.EdgeDeviceCount.Should().Be(0);
            _ = this.portalMetric.ConnectedEdgeDeviceCount.Should().Be(3);
            _ = this.portalMetric.FailedDeploymentCount.Should().Be(1);
            this.mockRepository.VerifyAll();
        }


        [Test]
        public void EdgeDeviceMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConnectedEdgeDeviceCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(10);

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(c => c.ConnectionState == "Connected", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InternalServerErrorException(string.Empty));

            _ = this.mockConfigService.Setup(service => service.GetFailedDeploymentsCount()).ReturnsAsync(1);

            // Act
            _ = this.edgeDeviceMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.EdgeDeviceCount.Should().Be(10);
            _ = this.portalMetric.ConnectedEdgeDeviceCount.Should().Be(0);
            _ = this.portalMetric.FailedDeploymentCount.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void EdgeDeviceMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingFailedDeploymentCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(10);

            _ = this.mockEdgeDeviceRepository.Setup(c => c.CountAsync(c => c.ConnectionState == "Connected", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(3);

            _ = this.mockConfigService.Setup(service => service.GetFailedDeploymentsCount()).ThrowsAsync(new InternalServerErrorException("test"));

            // Act
            _ = this.edgeDeviceMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.EdgeDeviceCount.Should().Be(10);
            _ = this.portalMetric.ConnectedEdgeDeviceCount.Should().Be(3);
            _ = this.portalMetric.FailedDeploymentCount.Should().Be(0);
            this.mockRepository.VerifyAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
