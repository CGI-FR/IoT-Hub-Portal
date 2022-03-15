// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceServiceTests
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;
        private Mock<ServiceClient> mockServiceClient;
        private Mock<ILogger<DeviceService>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockServiceClient = this.mockRepository.Create<ServiceClient>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceService>>();
        }

        private DeviceService CreateService()
        {
            return new DeviceService(
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object);
        }

        [Test]
        public async Task GetAllEdgeDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();

            var resultReturned = false;

            _ = mockQuery.SetupGet(c => c.HasMoreResults)
                .Returns(!resultReturned);

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
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
        public async Task GetAllDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync(It.IsAny<QueryOptions>()))
                .ReturnsAsync(new QueryResponse<Twin>(new[]{
                    new Twin(Guid.NewGuid().ToString())
                    }, string.Empty));

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ totalNumber: 10}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = false"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var expected = new Device();

            _ = this.mockRegistryManager.Setup(c => c.GetDeviceAsync(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(expected);

            // Act
            var result = await service.GetDevice(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var expected = new Twin();

            _ = this.mockRegistryManager.Setup(c => c.GetTwinAsync(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(expected);

            // Act
            var result = await service.GetDeviceTwin(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinWithModuleStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var mockQuery = this.mockRepository.Create<IQuery>();
            var resultReturned = false;

            _ = mockQuery.SetupGet(c => c.HasMoreResults)
                .Returns(!resultReturned);

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
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
        public async Task CreateDeviceWithTwinStateUnderTestExpectedBehavior(bool isEdge, DeviceStatus isEnabled)
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var twin = new Twin();

            _ = this.mockRegistryManager
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
        public async Task DeleteDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.RemoveDeviceAsync(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            // Act
            await service.DeleteDevice(deviceId);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var device = new Device();

            _ = this.mockRegistryManager.Setup(c => c.UpdateDeviceAsync(It.Is<Device>(x => x == device)))
                .ReturnsAsync(new Device());

            // Act
            var result = await service.UpdateDevice(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(result, device);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var twin = new Twin()
            {
                ETag = Guid.NewGuid().ToString(),
            };

            _ = this.mockRegistryManager.Setup(c => c.UpdateTwinAsync(
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
        public async Task ExecuteC2DMethodStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var method = new CloudToDeviceMethod(Guid.NewGuid().ToString());

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
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
