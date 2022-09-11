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
    public class ConcentratorMetricExporterServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricExporterService concentratorMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<ConcentratorMetricExporterService>> mockLogger;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");
        private readonly Counter connectedConcentratorCounter = Metrics.CreateCounter(MetricName.ConnectedConcentratorCount, "Connected concentrators count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricExporterService>>();

            this.portalMetric = new PortalMetric();

            this.concentratorMetricExporterService =
                new ConcentratorMetricExporterService(this.mockLogger.Object, this.portalMetric);
        }

        [Test]
        public void ConcentratorMetricExporterServiceShouldExportConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            this.portalMetric.ConcentratorCount = 15;
            this.portalMetric.ConnectedConcentratorCount = 8;

            // Act
            _ = this.concentratorMetricExporterService.Execute(null);

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
