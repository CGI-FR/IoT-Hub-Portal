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
    public class DeviceMetricExporterServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private DeviceMetricExporterService deviceMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<DeviceMetricExporterService>> mockLogger;

        private readonly Counter deviceCounter = Metrics.CreateCounter(MetricName.DeviceCount, "Devices count");
        private readonly Counter connectedDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedDeviceCount, "Connected devices count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceMetricExporterService>>();

            this.portalMetric = new PortalMetric();

            this.deviceMetricExporterService =
                new DeviceMetricExporterService(this.mockLogger.Object, this.portalMetric);
        }

        [Test]
        public void DeviceMetricExporterServiceShouldExportDeviceCountAndConnectedConnectedDeviceCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            this.portalMetric.DeviceCount = 15;
            this.portalMetric.ConnectedDeviceCount = 8;

            // Act
            _ = this.deviceMetricExporterService.Execute(null);

            // Assert
            _ = this.deviceCounter.Value.Should().Be(this.portalMetric.DeviceCount);
            _ = this.connectedDeviceCounter.Value.Should().Be(this.portalMetric.ConnectedDeviceCount);
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
