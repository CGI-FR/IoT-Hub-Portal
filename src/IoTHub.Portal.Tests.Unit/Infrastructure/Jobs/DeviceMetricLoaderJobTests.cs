// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceMetricLoaderJobTests : IDisposable
    {
        private MockRepository mockRepository;
        private DeviceMetricLoaderJob deviceMetricLoaderService;
        private PortalMetric portalMetric;

        private Mock<ILogger<DeviceMetricLoaderJob>> mockLogger;
        private Mock<IExternalDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceMetricLoaderJob>>();
            this.mockDeviceService = this.mockRepository.Create<IExternalDeviceService>();

            this.portalMetric = new PortalMetric();

            this.deviceMetricLoaderService =
                new DeviceMetricLoaderJob(this.mockLogger.Object, this.portalMetric, this.mockDeviceService.Object);
        }

        [Test]
        public void DeviceMetricLoaderServiceShouldLoadDeviceCountAndConnectedDeviceCountMetrics()
        {
            // Arrange
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ReturnsAsync(3);

            // Act
            _ = this.deviceMetricLoaderService.Execute(null);

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

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ReturnsAsync(3);

            // Act
            _ = this.deviceMetricLoaderService.Execute(null);

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

            _ = this.mockDeviceService.Setup(service => service.GetDevicesCount()).ReturnsAsync(10);
            _ = this.mockDeviceService.Setup(service => service.GetConnectedDevicesCount()).ThrowsAsync(new InternalServerErrorException("test"));

            // Act
            _ = this.deviceMetricLoaderService.Execute(null);

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
