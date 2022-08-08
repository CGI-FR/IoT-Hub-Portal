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
    using Microsoft.Extensions.DependencyInjection;
    using Server.Exceptions;

    [TestFixture]
    public class ConcentratorMetricLoaderServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private ConcentratorMetricLoaderService concentratorMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<ConcentratorMetricLoaderService>> mockLogger;
        private Mock<ConfigHandler> mockConfigHandler;
        private Mock<IDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<ConcentratorMetricLoaderService>>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();

            this.portalMetric = new PortalMetric();

            var services = new ServiceCollection();

            _ = services.AddTransient(_ => this.mockDeviceService.Object);

            this.concentratorMetricLoaderService =
                new ConcentratorMetricLoaderService(this.mockLogger.Object, this.mockConfigHandler.Object, this.portalMetric, services.BuildServiceProvider());
        }

        [Test]
        public void ConcentratorMetricLoaderServiceShouldLoadConcentratorCountAndConnectedConcentratorCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedConcentratorsCount()).ReturnsAsync(3);

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.concentratorMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(10);
            _ = this.portalMetric.ConnectedConcentratorCount.Should().Be(3);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ConcentratorMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConcentratorCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ThrowsAsync(new InternalServerErrorException("test"));
            _ = this.mockDeviceService.Setup(service => service.GetConnectedConcentratorsCount()).ReturnsAsync(3);

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.concentratorMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(0);
            _ = this.portalMetric.ConnectedConcentratorCount.Should().Be(3);
            this.mockRepository.VerifyAll();
        }


        [Test]
        public void ConcentratorMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConnectedConcentratorCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetConcentratorsCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedConcentratorsCount()).ThrowsAsync(new InternalServerErrorException("test"));

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.concentratorMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.ConcentratorCount.Should().Be(10);
            _ = this.portalMetric.ConnectedConcentratorCount.Should().Be(0);
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
