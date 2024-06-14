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
    public class ConcentratorMetricExporterJobTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricExporterJob concentratorMetricExporterService;
        private PortalMetric portalMetric;

        private Mock<ILogger<ConcentratorMetricExporterJob>> mockLogger;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricExporterJob>>();

            this.portalMetric = new PortalMetric();

            this.concentratorMetricExporterService =
                new ConcentratorMetricExporterJob(this.mockLogger.Object, this.portalMetric);
        }

        [Test]
        public void ConcentratorMetricExporterServiceShouldExportConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            this.portalMetric.ConcentratorCount = 15;

            // Act
            _ = this.concentratorMetricExporterService.Execute(null);

            // Assert
            _ = this.concentratorCounter.Value.Should().Be(this.portalMetric.ConcentratorCount);
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
