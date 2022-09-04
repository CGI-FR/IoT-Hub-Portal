// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Threading;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Portal.Server.Exceptions;

    [TestFixture]
    public class DeviceMetricLoaderServiceTests : IDisposable
    {
        private MockRepository mockRepository;
        private DeviceMetricLoaderService deviceMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<DeviceMetricLoaderService>> mockLogger;
        private Mock<ConfigHandler> mockConfigHandler;
        private Mock<IDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceMetricLoaderService>>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();

            this.portalMetric = new PortalMetric();

            var services = new ServiceCollection();

            _ = services.AddTransient(_ => this.mockDeviceService.Object);

            this.deviceMetricLoaderService =
                new DeviceMetricLoaderService(this.mockLogger.Object, this.mockConfigHandler.Object, this.portalMetric, services.BuildServiceProvider());
        }

        [Test]
        public void DeviceMetricLoaderServiceShouldLoadDeviceCountAndConnectedDeviceCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ReturnsAsync(3);

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.deviceMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.DeviceCount.Should().Be(10);
            _ = this.portalMetric.ConnectedDeviceCount.Should().Be(3);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void DeviceMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingDeviceCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ReturnsAsync(3);

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.deviceMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.DeviceCount.Should().Be(0);
            _ = this.portalMetric.ConnectedDeviceCount.Should().Be(3);
            this.mockRepository.VerifyAll();
        }


        [Test]
        public void DeviceMetricLoaderServiceShouldHandleInternalServerErrorExceptionWhenLoadingConnectedDeviceCountMetric()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            _ = this.mockConfigHandler.Setup(handler => handler.MetricLoaderRefreshIntervalInMinutes).Returns(1);

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));

            using var cancellationToken = new CancellationTokenSource();

            // Act
            _ = this.deviceMetricLoaderService.StartAsync(cancellationToken.Token);
            cancellationToken.Cancel();

            // Assert
            _ = this.portalMetric.DeviceCount.Should().Be(10);
            _ = this.portalMetric.ConnectedDeviceCount.Should().Be(0);
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
