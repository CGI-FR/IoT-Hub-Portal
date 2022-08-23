// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANConcentratorServiceTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANConcentratorService>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IRouterConfigManager> mockRouterConfigManager;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANConcentratorService>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockRouterConfigManager = this.mockRepository.Create<IRouterConfigManager>();
            this.mockConcentratorTwinMapper = this.mockRepository.Create<IConcentratorTwinMapper>();
        }

        private LoRaWANConcentratorService CreateLoRaWANConcentratorService()
        {
            return new LoRaWANConcentratorService(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockConcentratorTwinMapper.Object,
                this.mockRouterConfigManager.Object);
        }

        [Test]
        public async Task CreateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorService();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            var routerConfig = new RouterConfig();
            var mockResult = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };

            var twin = new Twin
            {
                DeviceId = concentrator.DeviceId,
            };

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == twin.DeviceId),
                It.Is<bool>(x => !x),
                It.Is<Twin>(x => x.DeviceId == twin.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            _ = this.mockRouterConfigManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(routerConfig);

            _ = this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceAlreadyExistCreateDeviceAsyncWithValidArgumentShouldThrowAnDeviceAlreadyExistsException()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorService();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            var routerConfig = new RouterConfig();

            var twin = new Twin
            {
                DeviceId = concentrator.DeviceId,
            };

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == twin.DeviceId),
                It.Is<bool>(x => !x),
                It.Is<Twin>(x => x.DeviceId == twin.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ThrowsAsync(new DeviceAlreadyExistsException(""));

            _ = this.mockRouterConfigManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(routerConfig);

            _ = this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            _ = Assert.ThrowsAsync<DeviceAlreadyExistsException>(async () => await concentratorController.CreateDeviceAsync(concentrator));

            // Assert
            this.mockRepository.VerifyAll();
        }
    }
}
