// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConcentratorMetricLoaderServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricLoaderService concentratorMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<ConcentratorMetricLoaderService>> mockLogger;
        private Mock<IExternalDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricLoaderService>>();
            this.mockDeviceService = this.mockRepository.Create<IExternalDeviceService>();

            this.portalMetric = new PortalMetric();

            this.concentratorMetricLoaderService =
                new ConcentratorMetricLoaderService(this.mockLogger.Object, this.portalMetric, this.mockDeviceService.Object);
        }

        [Test]
        public void ConcentratorMetricLoaderServiceShouldLoadConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ReturnsAsync(10);

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

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ThrowsAsync(new InternalServerErrorException("test"));

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

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ReturnsAsync(10);

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
