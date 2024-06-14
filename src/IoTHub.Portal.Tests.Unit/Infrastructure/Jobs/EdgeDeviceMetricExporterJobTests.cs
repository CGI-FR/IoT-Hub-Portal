// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using IoTHub.Portal.Domain.Shared.Constants;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Prometheus;

    [TestFixture]
    public class EdgeDeviceMetricExporterJobTests : IDisposable
    {
        private MockRepository mockRepository;
        private EdgeDeviceMetricExporterJob edgeDeviceMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<EdgeDeviceMetricExporterJob>> mockLogger;

        private readonly Counter edgeDeviceCounter = Metrics.CreateCounter(MetricName.EdgeDeviceCount, "Edge devices count");
        private readonly Counter connectedEdgeDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedEdgeDeviceCount, "Connected edge devices count");
        private readonly Counter failedDeploymentCount = Metrics.CreateCounter(MetricName.FailedDeploymentCount, "Failed deployments count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDeviceMetricExporterJob>>();

            this.portalMetric = new PortalMetric();

            this.edgeDeviceMetricExporterService =
                new EdgeDeviceMetricExporterJob(this.mockLogger.Object, this.portalMetric);
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
