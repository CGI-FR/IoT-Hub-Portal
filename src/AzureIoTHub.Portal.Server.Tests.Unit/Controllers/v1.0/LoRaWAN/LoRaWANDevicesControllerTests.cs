using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10.LoRaWAN
{
    [TestFixture]
    public class LoRaWANDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceProvisioningServiceManager> mockProvisioningServiceManager;
        private Mock<ILogger<LoRaWANDevicesController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<ILoraDeviceMethodManager> mockLoraDeviceMethodManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<TableClient> mockCommandsTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockLoraDeviceMethodManager = this.mockRepository.Create<ILoraDeviceMethodManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>(); 
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();

        }

        private LoRaWANDevicesController CreateLoRaWANDevicesController()
        {
            return new LoRaWANDevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTagService.Object,
                this.mockDeviceTwinMapper.Object,
                this.mockTableClientFactory.Object,
                this.mockLoraDeviceMethodManager.Object,
                this.mockDeviceModelCommandMapper.Object,
                this.mockProvisioningServiceManager.Object);
        }

        [Test]
        public async Task ExecuteCommand_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var loRaWANDevicesController = this.CreateLoRaWANDevicesController();
            string deviceId = Guid.NewGuid().ToString();
            string commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{ commandId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(Guid.NewGuid().ToString(), commandId)
                                    }, null, mockResponse.Object)
                    }));

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));                

            // Act
            var result = await loRaWANDevicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommand_Failed_Should_Return_Http400()
        {
            // Arrange
            var loRaWANDevicesController = this.CreateLoRaWANDevicesController();
            string deviceId = Guid.NewGuid().ToString();
            string commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{ commandId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(Guid.NewGuid().ToString(), commandId)
                                    }, null, mockResponse.Object)
                    }));

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = await loRaWANDevicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }


        [Test]
        public async Task GetList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            int count = 100;
            TwinCollection twinCollection = new TwinCollection();
            twinCollection["deviceType"] = "test";

            this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.Is<string>(x => x == "LoRa Concentrator")))
                .ReturnsAsync(Enumerable.Range(0, 100).Select(x => new Twin
                {
                    DeviceId = x.ToString(),
                    Tags = twinCollection
                }));

            this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceListItem(It.IsAny<Twin>()))
                .Returns<Twin>(x => new DeviceListItem
                {
                    DeviceID = x.DeviceId
                });

            // Act
            var result = await devicesController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            string deviceID = "aaa";

            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceID)))
                .Returns<string>(x => Task.FromResult(new Twin(x)));

            this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(),null))
                .Returns<Twin>(x => new LoRaDeviceDetails
                {
                    DeviceID = x.DeviceId,
                    AppEUI = Guid.NewGuid().ToString(),
                    AppKey = Guid.NewGuid().ToString(),
                    SensorDecoder = Guid.NewGuid().ToString(),
                });

            // Act
            var result = await devicesController.Get(deviceID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(deviceID, result.DeviceID);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            LoRaDeviceDetails device = new LoRaDeviceDetails
            {
                DeviceID = "aaa",
            };

            Twin twin = null;

            this.mockDeviceTwinMapper.Setup(c => c.UpdateTwin(It.Is<Twin>(x => x.DeviceId == device.DeviceID), It.Is<LoRaDeviceDetails>(x => x == device)))
                .Callback<Twin, LoRaDeviceDetails>((t, d) => twin = t);

            this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(It.Is<string>(x => x == device.DeviceID), It.Is<bool>(x => !x), It.Is<Twin>(x => x == twin), It.Is<DeviceStatus>(x => x == (device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled))))
                .ReturnsAsync(new BulkRegistryOperationResult { IsSuccessful = true });

            // Act
            var result = await devicesController.CreateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsync_ModelNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            LoRaDeviceDetails device = new LoRaDeviceDetails
            {
                DeviceID = "aaa",
            };


            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = await devicesController.CreateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            LoRaDeviceDetails device = new LoRaDeviceDetails
            {
                DeviceID = "aaa"
            };

            Device item = new Device(device.DeviceID);
            Twin twin = new Twin(device.DeviceID);

            this.mockDeviceService.Setup(c => c.GetDevice(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(item);
            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(twin);
            this.mockDeviceService.Setup(c => c.UpdateDevice(It.Is<Device>(x => x.Id == item.Id)))
                .ReturnsAsync(item);
            this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(It.Is<string>(x => x == device.DeviceID), It.Is<Twin>(x => x.DeviceId == device.DeviceID)))
                .ReturnsAsync(twin);

            this.mockDeviceTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(x => x == twin), It.Is<LoRaDeviceDetails>(x => x == device)));

            // Act
            var result = await devicesController.UpdateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsync_ModelNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            LoRaDeviceDetails device = new LoRaDeviceDetails
            {
                DeviceID = "aaa"
            };

            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = await devicesController.UpdateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            string deviceID = "aaa";

            this.mockDeviceService.Setup(c => c.DeleteDevice(It.Is<string>(x => x == deviceID)))
                .Returns(Task.CompletedTask);

            // Act
            var result = await devicesController.Delete(deviceID);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEnrollmentCredentials_Should_Always_Return_404()
        {
            // Arrange
            var devicesController = this.CreateLoRaWANDevicesController();
            string deviceID = "aaa";

            // Act
            var response = await devicesController.GetCredentials(deviceID);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);
            this.mockRepository.VerifyAll();
        }
    }
}
