using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class ConcentratorsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IRouterConfigManager> mockRouterManager;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<ILogger<LoRaWANConcentratorsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANConcentratorsController>>();
            this.mockRouterManager = this.mockRepository.Create<IRouterConfigManager>();
            this.mockConcentratorTwinMapper = this.mockRepository.Create<IConcentratorTwinMapper>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
        }

        private LoRaWANConcentratorsController CreateController()
        {
            return new LoRaWANConcentratorsController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockRouterManager.Object,
                this.mockConcentratorTwinMapper.Object);
        }

        [Test]
        public async Task GetAllDeviceConcentrator_WithNoArgument_ReturnConcentratorList()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["deviceType"] = "LoRa Concentrator";
            var concentrator = new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceType = "LoRa Concentrator"
            };

            this.mockDeviceService.Setup(x => x.GetAllDevice(
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(new[]
                {
                    twin
                });

            this.mockConcentratorTwinMapper.Setup(x => x.CreateDeviceDetails(It.Is<Twin>(c => c.DeviceId == twin.DeviceId)))
                .Returns(concentrator);

            var concentratorController = this.CreateController();

            // Act
            var result = await concentratorController.GetAllDeviceConcentrator();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            var deviceList = okObjectResult.Value as IEnumerable<Concentrator>;
            Assert.IsNotNull(deviceList);
            Assert.AreEqual(1, deviceList.Count());
            var device = deviceList.First();
            Assert.IsNotNull(device);
            Assert.AreEqual(twin.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceConcentrator_With_Valid_Argument_Should_Return_Concentrator()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["deviceType"] = "LoRa Concentrator";
            var concentrator = new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceType = "LoRa Concentrator"
            };

            this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
                .ReturnsAsync(twin);

            this.mockConcentratorTwinMapper.Setup(x => x.CreateDeviceDetails(It.Is<Twin>(c => c.DeviceId == twin.DeviceId)))
                .Returns(concentrator);

            var concentratorController = this.CreateController();

            // Act
            var result = await concentratorController.GetDeviceConcentrator(twin.DeviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<Concentrator>(okObjectResult.Value);
            var device = okObjectResult.Value as Concentrator;
            Assert.IsNotNull(device);
            Assert.AreEqual(twin.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsync_With_Valid_Argument_Should_Return_OkResult()
        {
            // Arrange
            var concentratorController = this.CreateController();
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

            this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == twin.DeviceId),
                It.Is<bool>(x => !x),
                It.Is<Twin>(x => x.DeviceId == twin.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            this.mockRouterManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(routerConfig);

            this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));
            this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<BulkRegistryOperationResult>(okObjectResult.Value);
            Assert.AreEqual(mockResult, okObjectResult.Value);
        }

        [Test]
        public async Task UpdateDeviceAsync_With_Valid_Argument_Should_Return_OkResult()
        {
            // Arrange
            var concentratorController = this.CreateController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true,
                RouterConfig = new RouterConfig()
            };

            var twin = new Twin
            {
                DeviceId = concentrator.DeviceId,
            };

            var device = new Device(concentrator.DeviceId);

            this.mockRouterManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(concentrator.RouterConfig);
            this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));

            this.mockDeviceService.Setup(x => x.GetDevice(It.Is<string>(c => c == concentrator.DeviceId)))
                .ReturnsAsync(device);
            this.mockDeviceService.Setup(x => x.UpdateDevice(It.Is<Device>(c => c.Id == concentrator.DeviceId)))
                .ReturnsAsync(device);
            this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == concentrator.DeviceId)))
                .ReturnsAsync(twin);
            this.mockDeviceService.Setup(x => x.UpdateDeviceTwin(It.Is<string>(c => c == concentrator.DeviceId), It.Is<Twin>(c => c.DeviceId == concentrator.DeviceId)))
                .ReturnsAsync(twin);

            // Act
            var result = await concentratorController.UpdateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
        }

        [Test]
        public async Task DeleteDeviceAsync()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }
    }
}
