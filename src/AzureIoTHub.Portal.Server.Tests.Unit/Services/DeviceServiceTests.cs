using AzureIoTHub.Portal.Server.Services;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Services
{
    [TestFixture]
    public class DeviceServiceTests
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;
        private Mock<ServiceClient> mockServiceClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockServiceClient = this.mockRepository.Create<ServiceClient>();
        }

        private DeviceService CreateService()
        {
            return new DeviceService(
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object);
        }

        [Test]
        public async Task GetAllEdgeDevice_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();

            var resultReturned = false;

            mockQuery.SetupGet(c => c.HasMoreResults)
                .Returns(!resultReturned);

            mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDevice_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();

            var resultReturned = false;

            mockQuery.SetupGet(c => c.HasMoreResults)
                .Returns(!resultReturned);

            mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = false")))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetAllDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevice_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string deviceId = Guid.NewGuid().ToString();
            var expected = new Device();

            this.mockRegistryManager.Setup(c => c.GetDeviceAsync(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(expected);

            // Act
            var result = await service.GetDevice(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwin_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string deviceId = Guid.NewGuid().ToString();
            var expected = new Twin();

            this.mockRegistryManager.Setup(c => c.GetTwinAsync(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(expected);

            // Act
            var result = await service.GetDeviceTwin(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinWithModule_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string deviceId = Guid.NewGuid().ToString();

            var mockQuery = this.mockRepository.Create<IQuery>();
            var resultReturned = false;

            mockQuery.SetupGet(c => c.HasMoreResults)
                .Returns(!resultReturned);

            mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']")))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetDeviceTwinWithModule(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<Twin>(result);
            this.mockRepository.VerifyAll();
        }

        [TestCase(false, DeviceStatus.Enabled)]
        [TestCase(true, DeviceStatus.Enabled)]
        [TestCase(false, DeviceStatus.Disabled)]
        [TestCase(true, DeviceStatus.Disabled)]
        public async Task CreateDeviceWithTwin_StateUnderTest_ExpectedBehavior(bool isEdge, DeviceStatus isEnabled)
        {
            // Arrange
            var service = this.CreateService();
            string deviceId = Guid.NewGuid().ToString();
            Twin twin = new Twin();

            this.mockRegistryManager
                .Setup(c => c.AddDeviceWithTwinAsync(It.Is<Device>(x => x.Id == deviceId
                                                                    && x.Capabilities.IotEdge == isEdge
                                                                    && x.Status == isEnabled),
                                                     It.Is<Twin>(x => x == twin)))
                .ReturnsAsync(new BulkRegistryOperationResult
                {
                    IsSuccessful = true
                });

            // Act
            var result = await service.CreateDeviceWithTwin(
                deviceId,
                isEdge,
                twin,
                isEnabled);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string deviceId = Guid.NewGuid().ToString();

            this.mockRegistryManager.Setup(c => c.RemoveDeviceAsync(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            // Act
            await service.DeleteDevice(deviceId);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var device = new Device();

            this.mockRegistryManager.Setup(c => c.UpdateDeviceAsync(It.Is<Device>(x => x == device)))
                .ReturnsAsync(new Device());

            // Act
            var result = await service.UpdateDevice(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(result, device);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceTwin_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var twin = new Twin()
            {
                ETag = Guid.NewGuid().ToString(),
            };

            this.mockRegistryManager.Setup(c => c.UpdateTwinAsync(
                It.Is<string>(x => x == deviceId), 
                It.Is<Twin>(x => x == twin), 
                It.Is<string>(x => x == twin.ETag)))
                .ReturnsAsync(new Twin());

            // Act
            var result = await service.UpdateDeviceTwin(deviceId, twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(result, twin);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteC2DMethod_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            CloudToDeviceMethod method = new CloudToDeviceMethod(Guid.NewGuid().ToString());

            this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == "$edgeAgent"),
                It.Is<CloudToDeviceMethod>(x => x == method), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            // Act
            var result = await service.ExecuteC2DMethod(deviceId, method);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.Status);
            this.mockRepository.VerifyAll();
        }
    }
}
