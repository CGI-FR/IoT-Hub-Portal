// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using AzureIoTHub.Portal.Server;
    using System;
    using System.Threading;
    using NUnit.Framework;
    using Microsoft.Extensions.Logging;
    using Moq;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Prometheus;
    using Portal.Server.Constants;

    [TestFixture]
    public class ConcentratorMetricExporterServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricExporterService concentratorMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<ConcentratorMetricExporterService>> mockLogger;
        private Mock<ConfigHandler> mockConfigHandler;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");
        private readonly Counter connectedConcentratorCounter = Metrics.CreateCounter(MetricName.ConnectedConcentratorCount, "Connected concentrators count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricExporterService>>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();

            this.portalMetric = new PortalMetric();

            this.concentratorMetricExporterService =
                new ConcentratorMetricExporterService(this.mockLogger.Object, this.mockConfigHandler.Object, this.portalMetric);
        }

        [Test]
        public void ConcentratorMetricExporterServiceShouldExportConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricExporterRefreshIntervalInSeconds).Returns(1);

            this.portalMetric.ConcentratorCount = 15;
            this.portalMetric.ConnectedConcentratorCount = 8;

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.concentratorMetricExporterService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.concentratorCounter.Value.Should().Be(this.portalMetric.ConcentratorCount);
            _ = this.connectedConcentratorCounter.Value.Should().Be(this.portalMetric.ConnectedConcentratorCount);
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
