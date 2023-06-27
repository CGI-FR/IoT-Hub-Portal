// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConcentratorMetricLoaderJobTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricLoaderJob concentratorMetricLoaderService;
        private PortalMetricDto portalMetric;

        private Mock<ILogger<ConcentratorMetricLoaderJob>> mockLogger;
        private Mock<IConcentratorRepository> mockConcentratorRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricLoaderJob>>();
            this.mockConcentratorRepository = this.mockRepository.Create<IConcentratorRepository>();

            this.portalMetric = new PortalMetricDto();

            this.concentratorMetricLoaderService =
                new ConcentratorMetricLoaderJob(this.mockLogger.Object, this.portalMetric, this.mockConcentratorRepository.Object);
        }

        [Test]
        public void ConcentratorMetricLoaderServiceShouldLoadConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockConcentratorRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            // Act
            _ = this.concentratorMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(10);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ConcentratorMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConcentratorCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockConcentratorRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InternalServerErrorException("test"));

            // Act
            _ = this.concentratorMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(0);
            this.mockRepository.VerifyAll();
        }


        [Test]
        public void ConcentratorMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConnectedConcentratorCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockConcentratorRepository.Setup(c => c.CountAsync(null, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(10);

            // Act
            _ = this.concentratorMetricLoaderService.Execute(null);

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(10);
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
