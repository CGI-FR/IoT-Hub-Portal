using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class DevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DevicesController>> mockLogger;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTwinMapper<DeviceListItem, DeviceDetails>> mockDeviceTwinMapper;
        private Mock<ILoraDeviceMethodManager> mockLoraDeviceMethodManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DevicesController>>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, DeviceDetails>>();
            this.mockLoraDeviceMethodManager = this.mockRepository.Create<ILoraDeviceMethodManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
        }

        private DevicesController CreateDevicesController()
        {
            return new DevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTwinMapper.Object);
        }

        [Test]
        public async Task GetList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
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
            var devicesController = this.CreateDevicesController();
            string deviceID = "aaa";

            this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceID)))
                .Returns<string>(x => Task.FromResult(new Twin(x)));

            this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>()))
                .Returns<Twin>(x => new DeviceDetails
                {
                    DeviceID = x.DeviceId
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
            var devicesController = this.CreateDevicesController();
            DeviceDetails device = new DeviceDetails
            {
                DeviceID = "aaa",
            };

            Twin twin = null;

            this.mockDeviceTwinMapper.Setup(c => c.UpdateTwin(It.Is<Twin>(x => x.DeviceId == device.DeviceID), It.Is<DeviceDetails>(x => x == device)))
                .Callback<Twin, DeviceDetails>((t, d) => twin = t);

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
            var devicesController = this.CreateDevicesController();
            DeviceDetails device = new DeviceDetails
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
            var devicesController = this.CreateDevicesController();
            DeviceDetails device = new DeviceDetails
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

            this.mockDeviceTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(x => x == twin), It.Is<DeviceDetails>(x => x == device)));

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
            var devicesController = this.CreateDevicesController();
            DeviceDetails device = new DeviceDetails
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
            var devicesController = this.CreateDevicesController();
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
        public async Task ExecuteCommand_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            //// Arrange
            //var devicesController = this.CreateDevicesController();
            //string deviceId = null;
            //string commandId = null;

            //// Act
            //var result = await devicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.Inconclusive();
            this.mockRepository.VerifyAll();
        }
    }
}
