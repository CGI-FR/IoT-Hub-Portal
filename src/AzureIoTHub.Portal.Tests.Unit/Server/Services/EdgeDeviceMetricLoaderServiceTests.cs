// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Threading;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceMetricLoaderServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private EdgeDeviceMetricLoaderService edgeDeviceMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<EdgeDeviceMetricLoaderService>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IConfigService> mockConfigService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDeviceMetricLoaderService>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockConfigService = this.mockRepository.Create<IConfigService>();

            this.portalMetric = new PortalMetric();

            this.edgeDeviceMetricLoaderService =
                new EdgeDeviceMetricLoaderService(this.mockLogger.Object, this.portalMetric, this.mockDeviceService.Object, this.mockConfigService.Object);
        }

        [Test]
        public void EdgeDeviceMetricLoaderServiceShouldLoadEdgeDeviceCountAndConnectedEdgeDeviceCountAndFailedDeploymentCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(service => service.GetEdgeDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedEdgeDevicesCount()).ReturnsAsync(3);
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

            _ = this.mockDeviceService.Setup(service => service.GetEdgeDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));
            _ = this.mockDeviceService.Setup(service => service.GetConnectedEdgeDevicesCount()).ReturnsAsync(3);
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

            _ = this.mockDeviceService.Setup(service => service.GetEdgeDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedEdgeDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));
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

            _ = this.mockDeviceService.Setup(service => service.GetEdgeDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedEdgeDevicesCount()).ReturnsAsync(3);
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
