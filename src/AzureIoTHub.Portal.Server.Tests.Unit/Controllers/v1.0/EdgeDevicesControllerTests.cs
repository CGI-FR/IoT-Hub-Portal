// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

    [TestFixture]
    public class EdgeDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceProvisioningServiceManager> mockProvisioningServiceManager;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<EdgeDevicesController>> mockLogger;
        private Mock<RegistryManager> mockRegistryManager;
        private Mock<IDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDevicesController>>();
            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
        }

        private EdgeDevicesController CreateEdgeDevicesController()
        {
            return new EdgeDevicesController(
                this.mockConfiguration.Object,
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockDeviceService.Object,
                this.mockProvisioningServiceManager.Object);
        }

        [Test]
        public async Task GetAllDevicesShouldReturnListOfEdgeDevices()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["purpose"] = "test";

            _ = this.mockDeviceService.Setup(x => x.GetAllEdgeDevice())
                .ReturnsAsync(new[]
                {
                    twin
                });

            _ = this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
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
        public async Task WhenSpecifyingIdGetShouldReturnTheEdgeDevice()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            var twin = new Twin(deviceId);
            twin.Tags["purpose"] = "bbb";
            twin.Tags["env"] = "fake";

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwinWithModule(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(twin);

            var edgeHubTwin = new Twin("edgeHub");
            edgeHubTwin.Properties.Reported["clients"] = new[]
            {
                1, 2
            };

            var mockQuery = this.mockRepository.Create<IQuery>();
            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new[] { edgeHubTwin });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
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
        public async Task WhenNotFoundShouldReturnNotFound()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .Throws(new DeviceNotFoundException(deviceId));

            // Act
            var result = await edgeDevicesController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var mockRegistrationCredentials = new EnrollmentCredentials
            {
                RegistrationID = "aaa",
                SymmetricKey = "dfhjkfdgh"
            };

            var mockTwin = new Twin("aaa");
            mockTwin.Tags["purpose"] = "bbb";

            _ = this.mockProvisioningServiceManager.Setup(c => c.GetEnrollmentCredentialsAsync("aaa", "bbb"))
                .ReturnsAsync(mockRegistrationCredentials);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(mockTwin);

            // Act
            var response = await edgeDevicesController.GetCredentials("aaa");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            var okObjectResult = (OkObjectResult)response.Result;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(mockRegistrationCredentials, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceNotExistGetEnrollmentCredentialsShouldReturnNotFound()
        {
            // Arrange
            var devicesController = this.CreateEdgeDevicesController();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync((Twin)null);

            // Act
            var response = await devicesController.GetCredentials("aaa");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundObjectResult>(response.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateGatewayAsyncStateUnderTestExpectedBehavior()
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

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == gateway.DeviceId),
                It.Is<bool>(x => x),
                It.Is<Twin>(x => x.DeviceId == gateway.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

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
        public async Task WhenDeviceAlreadyExistsPostShouldReturnBadRequest()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var gateway = new IoTEdgeDevice()
            {
                DeviceId = "aaa",
                Type = "lora"
            };

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == gateway.DeviceId),
                It.Is<bool>(x => x),
                It.Is<Twin>(x => x.DeviceId == gateway.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .Throws(new DeviceAlreadyExistsException(gateway.DeviceId));

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await edgeDevicesController.CreateGatewayAsync(gateway);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateDeviceAsyncStateUnderTestExpectedBehavior()
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

            _ = this.mockDeviceService.Setup(c => c.GetDevice(It.Is<string>(x => x == gateway.DeviceId)))
                .ReturnsAsync(new Device(gateway.DeviceId)
                {
                    Status = DeviceStatus.Disabled
                });

            _ = this.mockDeviceService.Setup(c => c.UpdateDevice(It.Is<Device>(x => x.Id == gateway.DeviceId && x.Status == DeviceStatus.Enabled)))
                .ReturnsAsync(new Device(gateway.DeviceId)
                {
                    Status = DeviceStatus.Enabled
                });

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == gateway.DeviceId)))
                .ReturnsAsync(mockTwin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(It.Is<string>(x => x == gateway.DeviceId), It.Is<Twin>(x => x == mockTwin)))
                .ReturnsAsync(mockTwin);

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

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
        public async Task DeleteDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.DeleteDevice(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);


            _ = this.mockLogger.Setup(c => c.Log(
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

        [TestCase("RestartModule", /*lang=json,strict*/ "{\"id\":\"aaa\",\"schemaVersion\":null}")]
        [TestCase("GetModuleLogs", /*lang=json,strict*/ "{\"schemaVersion\":null,\"items\":[{\"id\":\"aaa\",\"filter\":{\"tail\":10}}],\"encoding\":\"none\",\"contentType\":\"json\"}")]
        public async Task ExecuteMethodShouldExecuteC2DMethod(string methodName, string expected)
        {
            // Arrange
            var edgeDevicesController = this.CreateEdgeDevicesController();
            var module = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();


            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _ = this.mockDeviceService.Setup(c => c.ExecuteC2DMethod(
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
