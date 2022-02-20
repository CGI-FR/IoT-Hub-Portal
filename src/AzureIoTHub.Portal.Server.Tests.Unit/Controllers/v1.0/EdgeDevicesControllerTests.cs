using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
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
    public class EdgeDevicesControllerTests
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

        private EdgeDevicesController CreateEdgeDevicesController()
        {
            return new EdgeDevicesController(
                this.mockConfiguration.Object,
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockConnectionStringManager.Object,
                this.mockDeviceService.Object);
        }

        [Test]
        public async Task Get_All_Devices_Should_Return_List_Of_Edge_Devices()
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

            var edgeDevicesController = this.CreateEdgeDevicesController();

            // Act
            var result = await edgeDevicesController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<List<IoTEdgeListItem>>(okObjectResult.Value);
            var gatewayList = okObjectResult.Value as List<IoTEdgeListItem>;
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
        public async Task When_Specifying_Id_Get_Should_Return_The_Edge_Device()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            string deviceId = Guid.NewGuid().ToString();

            var twin = new Twin(deviceId);
            twin.Tags["purpose"] = "bbb";
            twin.Tags["env"] = "fake";

            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(twin);

            this.mockDeviceService.Setup(c => c.GetDeviceTwinWithModule(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(twin);

            this.mockConfiguration.Setup(c => c[It.Is<string>(x => x == "IoTDPS:ServiceEndpoint")])
                .Returns("fake.local");

            var edgeHubTwin = new Twin("edgeHub");
            edgeHubTwin.Properties.Reported["clients"] = new[]
            {
                1, 2
            };

            var mockQuery = this.mockRepository.Create<IQuery>();
            mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new[] { edgeHubTwin });

            this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']"),
                It.Is<int>(x => x == 1)))
                .Returns(mockQuery.Object);

            // Act
            var result = await edgeDevicesController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsAssignableFrom<IoTEdgeDevice>(okObjectResult.Value);
            var iotEdge = okObjectResult.Value as IoTEdgeDevice;
            Assert.IsNotNull(iotEdge);
            Assert.AreEqual(deviceId, iotEdge.DeviceId);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task When_Not_Found_Should_Return_Not_Found()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            string deviceId = Guid.NewGuid().ToString();

            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .Throws(new DeviceNotFoundException(deviceId));

            // Act
            var result = await edgeDevicesController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetSymmetricKey_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            this.mockConnectionStringManager.Setup(c => c.GetSymmetricKey("aaa", "bbb"))
                .ReturnsAsync("dfhjkfdgh");

            // Act
            var result = await edgeDevicesController.GetSymmetricKey("aaa", "bbb");

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
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var gateway = new IoTEdgeDevice()
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
            var result = await edgeDevicesController.CreateGatewayAsync(gateway);

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
        public async Task When_Device_Already_Exists_Post_Should_Return_Bad_Request()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var gateway = new IoTEdgeDevice()
            {
                DeviceId = "aaa",
                Type = "lora"
            };

            this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == gateway.DeviceId),
                It.Is<bool>(x => x),
                It.Is<Twin>(x => x.DeviceId == gateway.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .Throws(new DeviceAlreadyExistsException(gateway.DeviceId));

            this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await edgeDevicesController.CreateGatewayAsync(gateway);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var gateway = new IoTEdgeDevice()
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
            var result = await edgeDevicesController.UpdateDeviceAsync(gateway);

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
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            string deviceId = Guid.NewGuid().ToString();

            this.mockDeviceService.Setup(c => c.DeleteDevice(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = await edgeDevicesController.DeleteDeviceAsync(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [TestCase("RestartModule", "{\"id\":\"aaa\",\"schemaVersion\":null}")]
        [TestCase("GetModuleLogs", "{\"schemaVersion\":null,\"items\":[{\"id\":\"aaa\",\"filter\":{\"tail\":10}}],\"encoding\":\"none\",\"contentType\":\"json\"}")]
        public async Task ExecuteMethod_Should_Execute_C2D_Method(string methodName, string expected)
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            IoTEdgeModule module = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            string deviceId = Guid.NewGuid().ToString();

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            this.mockDeviceService.Setup(c => c.ExecuteC2DMethod(
                It.Is<string>(x => x == deviceId),
                It.Is<CloudToDeviceMethod>(x =>
                    x.MethodName == methodName
                    && x.GetPayloadAsJson() == expected
                )))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            // Act
            var result = await edgeDevicesController.ExecuteModuleMethod(
                module,
                deviceId,
                methodName);

            // Assert
            this.mockRepository.VerifyAll();
        }
    }
}
