// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using AzureIoTHub.Portal.Domain.Shared.Constants;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Prometheus;

    [TestFixture]
    public class EdgeDeviceMetricExporterServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private EdgeDeviceMetricExporterService edgeDeviceMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<EdgeDeviceMetricExporterService>> mockLogger;

        private readonly Counter edgeDeviceCounter = Metrics.CreateCounter(MetricName.EdgeDeviceCount, "Edge devices count");
        private readonly Counter connectedEdgeDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedEdgeDeviceCount, "Connected edge devices count");
        private readonly Counter failedDeploymentCount = Metrics.CreateCounter(MetricName.FailedDeploymentCount, "Failed deployments count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDeviceMetricExporterService>>();

            this.portalMetric = new PortalMetric();

            this.edgeDeviceMetricExporterService =
                new EdgeDeviceMetricExporterService(this.mockLogger.Object, this.portalMetric);
        }

        [Test]
        public void DeviceMetricExporterServiceShouldExportDeviceCountAndConnectedConnectedDeviceCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            this.portalMetric.EdgeDeviceCount = 15;
            this.portalMetric.ConnectedEdgeDeviceCount = 8;
            this.portalMetric.FailedDeploymentCount = 3;

            // Act
            _ = this.edgeDeviceMetricExporterService.Execute(null);

            // Assert
            _ = this.edgeDeviceCounter.Value.Should().Be(this.portalMetric.EdgeDeviceCount);
            _ = this.connectedEdgeDeviceCounter.Value.Should().Be(this.portalMetric.ConnectedEdgeDeviceCount);
            _ = this.failedDeploymentCount.Value.Should().Be(this.portalMetric.FailedDeploymentCount);
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
