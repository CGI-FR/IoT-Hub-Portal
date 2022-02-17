using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class GatewaysControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<EdgeDevicesController>> mockLogger;
        private Mock<RegistryManager> mockRegistryManager;
        private Mock<IConnectionStringManager> mockConnectionStringManager;
        private Mock<IDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDevicesController>>();
            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockConnectionStringManager = this.mockRepository.Create<IConnectionStringManager>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
        }

        private EdgeDevicesController CreateGatewaysController()
        {
            return new EdgeDevicesController(
                this.mockConfiguration.Object,
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockConnectionStringManager.Object,
                this.mockDeviceService.Object);
        }

        [Test]
        public async Task Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["purpose"] = "test";

            this.mockDeviceService.Setup(x => x.GetAllEdgeDevice())
                .ReturnsAsync(new[]
                {
                    twin
                });

            this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
                .ReturnsAsync(twin);

            var gatewaysController = this.CreateGatewaysController();

            // Act
            var result = await gatewaysController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<List<GatewayListItem>>(okObjectResult.Value);
            var gatewayList = okObjectResult.Value as List<GatewayListItem>;
            Assert.IsNotNull(gatewayList);
            Assert.AreEqual(1, gatewayList.Count);
            var gateway = gatewayList[0];
            Assert.IsNotNull(gateway);
            Assert.AreEqual(twin.DeviceId, gateway.DeviceId);
            Assert.AreEqual("test", gateway.Type);
            Assert.AreEqual(0, gateway.NbDevices);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetSymmetricKey_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var gatewaysController = this.CreateGatewaysController();
            this.mockConnectionStringManager.Setup(c => c.GetSymmetricKey("aaa", "bbb"))
                .ReturnsAsync("dfhjkfdgh");

            // Act
            var result = await gatewaysController.GetSymmetricKey("aaa", "bbb");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<string>(okObjectResult.Value);
            Assert.AreEqual("dfhjkfdgh", okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateGatewayAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var gatewaysController = this.CreateGatewaysController();
            var gateway = new Gateway()
            {
                DeviceId = "aaa",
                Type = "lora"
            };
            var mockResult = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };

            this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == gateway.DeviceId),
                It.Is<bool>(x => x),
                It.Is<Twin>(x => x.DeviceId == gateway.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await gatewaysController.CreateGatewayAsync(gateway);

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
        public async Task UpdateDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var gatewaysController = this.CreateGatewaysController();
            var gateway = new Gateway()
            {
                DeviceId = "aaa",
                Type = "lora",
                Environment = "prod",
                Status = DeviceStatus.Enabled.ToString()
            };

            var mockResult = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };

            var mockTwin = new Twin(gateway.DeviceId);
            mockTwin.Tags["env"] = "dev";

            this.mockDeviceService.Setup(c => c.GetDevice(It.Is<string>(x => x == gateway.DeviceId)))
                .ReturnsAsync(new Device(gateway.DeviceId)
                {
                    Status = DeviceStatus.Disabled
                });

            this.mockDeviceService.Setup(c => c.UpdateDevice(It.Is<Device>(x => x.Id == gateway.DeviceId && x.Status == DeviceStatus.Enabled)))
                .ReturnsAsync(new Device(gateway.DeviceId)
                 {
                     Status = DeviceStatus.Enabled
                 });

            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == gateway.DeviceId)))
                .ReturnsAsync(mockTwin);

            this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(It.Is<string>(x => x == gateway.DeviceId), It.Is<Twin>(x => x == mockTwin)))
                .ReturnsAsync(mockTwin);

            this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await gatewaysController.UpdateDeviceAsync(gateway);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<Twin>(okObjectResult.Value);
            Assert.AreEqual(mockTwin, okObjectResult.Value);
            Assert.AreEqual("prod", mockTwin.Tags["env"].ToString());
        }

        [Test]
        public async Task DeleteDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }

        [Test]
        public async Task ExecuteMethode_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }
    }
}
