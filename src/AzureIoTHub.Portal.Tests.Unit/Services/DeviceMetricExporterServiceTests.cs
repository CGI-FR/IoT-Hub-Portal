// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Threading;
    using NUnit.Framework;
    using Microsoft.Extensions.Logging;
    using Moq;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Prometheus;
    using Server.Constants;

    [TestFixture]
    public class DeviceMetricExporterServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private DeviceMetricExporterService deviceMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<DeviceMetricExporterService>> mockLogger;
        private Mock<ConfigHandler> mockConfigHandler;

        private readonly Counter deviceCounter = Metrics.CreateCounter(MetricName.DeviceCount, "Devices count");
        private readonly Counter connectedDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedDeviceCount, "Connected devices count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceMetricExporterService>>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();

            this.portalMetric = new PortalMetric();

            this.deviceMetricExporterService =
                new DeviceMetricExporterService(this.mockLogger.Object, this.mockConfigHandler.Object, this.portalMetric);
        }

        [Test]
        public void DeviceMetricExporterServiceShouldExportDeviceCountAndConnectedConnectedDeviceCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricExporterRefreshIntervalInSeconds).Returns(1);

            this.portalMetric.DeviceCount = 15;
            this.portalMetric.ConnectedDeviceCount = 8;

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.deviceMetricExporterService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

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
