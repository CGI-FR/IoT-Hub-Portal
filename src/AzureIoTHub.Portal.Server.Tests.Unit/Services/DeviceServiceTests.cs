// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Constants;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Server.Exceptions;

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
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync(It.IsAny<QueryOptions>()))
                .ReturnsAsync(new QueryResponse<Twin>(new[]{
                    new Twin(Guid.NewGuid().ToString())
                    }, string.Empty));

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = true"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDeviceShouldThrowInternalServerErrorExceptionWhenGettingDevices()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync(It.IsAny<QueryOptions>()))
                .ThrowsAsync(new Exception("test"));

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = true"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetAllEdgeDevice();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDeviceShouldThrowInternalServerErrorExceptionWhenGettingDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetAllEdgeDevice();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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
        public async Task GetAllDeviceShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingDevicesCount()
        {
            // Arrange
            var service = CreateService();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetAllDevice();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinWhithModuleShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.HasMoreResults)
                .Returns(true);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']")))
                .Returns(mockQuery.Object);


            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.GetDeviceTwinWithModule(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeviceShouldThrowInternalServerErrorExceptionWhenIssueOccursOneGettingDevices()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync(It.IsAny<QueryOptions>()))
                .ThrowsAsync(new Exception("test"));

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
            var act = () => service.GetAllDevice();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSpecifyingFilterToDeviceTypeGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND devices.tags.deviceType = 'filteredType'";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(filterDeviceType: "filteredType");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenExcludingDeviceTypeGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'filteredType')";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(excludeDeviceType: "filteredType");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingDeviceGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND (STARTSWITH(deviceId, 'test') OR (is_defined(tags.deviceName) AND STARTSWITH(tags.deviceName, 'test')))";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchText: "test");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingContinuationGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync(It.Is<QueryOptions>(x => x.ContinuationToken == "test")))
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
            var result = await service.GetAllDevice(continuationToken: "test");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingDisabledDeviceGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND status = 'disabled'";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchStatus: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingEnabledDeviceGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND status = 'enabled'";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchStatus: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingConnectedDeviceGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND connectionState = 'Connected'";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchState: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingDisconnectedDeviceGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND connectionState = 'Disconnected'";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchState: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSearchingTagGetAllDeviceShouldQueryToIoTHub()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            const string expectedAdditionalFilter = "AND is_defined(tags.testKey) AND STARTSWITH(tags.testKey, 'testValue')";

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false {expectedAdditionalFilter}")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice(searchTags: new System.Collections.Generic.Dictionary<string, string>
            {
                { "testKey", "testValue" }
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenFailedToDeserializeCountShouldReturnEmpty()
        {
            // Arrange
            var service = CreateService();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ fakeData: 10}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenCountReturnsZeroMatchingGetAllDevicesShouldReturnEmpty()
        {
            // Arrange
            var service = CreateService();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ totalNumber: 0}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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
        public async Task GetDeviceShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetDeviceAsync(It.Is<string>(x => x == deviceId)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.GetDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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
        public async Task GetDeviceTwinShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetTwinAsync(It.Is<string>(x => x == deviceId)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.GetDeviceTwin(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinWithModuleStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var mockQuery = this.mockRepository.Create<IQuery>();
            const bool resultReturned = false;

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
            var service = CreateService();
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
        public async Task CreateDeviceWithTwinShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var twin = new Twin();

            _ = this.mockRegistryManager
                .Setup(c => c.AddDeviceWithTwinAsync(It.Is<Device>(x => x.Id == deviceId),
                    It.Is<Twin>(x => x == twin)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.CreateDeviceWithTwin(deviceId, true, twin);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.RemoveDeviceAsync(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            // Act
            await service.DeleteDevice(deviceId);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.RemoveDeviceAsync(It.Is<string>(x => x == deviceId)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.DeleteDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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
        public async Task UpdateDeviceShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var device = new Device();

            _ = this.mockRegistryManager.Setup(c => c.UpdateDeviceAsync(It.Is<Device>(x => x == device)))
                .ThrowsAsync(new Exception());

            // Act
            var act = () => service.UpdateDevice(device);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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
        public async Task UpdateDeviceTwinShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var twin = new Twin()
            {
                ETag = Guid.NewGuid().ToString(),
            };

            _ = this.mockRegistryManager.Setup(c => c.UpdateTwinAsync(
                    It.Is<string>(x => x == deviceId),
                    It.Is<Twin>(x => x == twin),
                    It.Is<string>(x => x == twin.ETag)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.UpdateDeviceTwin(deviceId, twin);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteC2DMethodStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
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

        [Test]
        public async Task ExecuteC2DMethodShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var method = new CloudToDeviceMethod(Guid.NewGuid().ToString());

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                    It.Is<string>(x => x == deviceId),
                    It.Is<string>(x => x == "$edgeAgent"),
                    It.Is<CloudToDeviceMethod>(x => x == method),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => service.ExecuteC2DMethod(deviceId, method);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnLogsWhen200IsReturned()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = edgeModule.Version,
                items = new[]
                {
                    new
                    {
                        id = edgeModule.ModuleName,
                        filter = new
                        {
                            tail = 300
                        }
                    }
                },
                encoding = "none",
                contentType = "json"
            });

            _ = method.SetPayloadJson(payload);


            var logger = Mock.Of<ILogger<DeviceService>>();

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == "$edgeAgent"),
                It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            var deviceService = new DeviceService(
                logger,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object);

            // Act
            var result = await deviceService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Count().Should().Be(0);
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnEmptyLogsWhenNot200IsReturned()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = edgeModule.Version,
                items = new[]
                {
                    new
                    {
                        id = edgeModule.ModuleName,
                        filter = new
                        {
                            tail = 300
                        }
                    }
                },
                encoding = "none",
                contentType = "json"
            });

            _ = method.SetPayloadJson(payload);


            var logger = Mock.Of<ILogger<DeviceService>>();

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == "$edgeAgent"),
                It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 500
                });

            var deviceService = new DeviceService(
                logger,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object);

            // Act
            var result = await deviceService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Count().Should().Be(0);
        }

        [Test]
        public async Task GetEdgeDeviceLogsShouldInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = edgeModule.Version,
                items = new[]
                {
                    new
                    {
                        id = edgeModule.ModuleName,
                        filter = new
                        {
                            tail = 300
                        }
                    }
                },
                encoding = "none",
                contentType = "json"
            });

            _ = method.SetPayloadJson(payload);


            var logger = Mock.Of<ILogger<DeviceService>>();

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                    It.Is<string>(x => x == deviceId),
                    It.Is<string>(x => x == "$edgeAgent"),
                    It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("test"));

            var deviceService = new DeviceService(
                logger,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object);

            // Act
            var act = () => deviceService.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesCountShouldReturnDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetDevicesCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedDevicesCountCountShouldReturnConnectedDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND connectionState = 'Connected' AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetConnectedDevicesCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedDevicesCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingConnectedDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND connectionState = 'Connected' AND (NOT is_defined(tags.deviceType) OR devices.tags.deviceType != 'LoRa Concentrator')")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetConnectedDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDevicesCountCountShouldReturnEdgeDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetEdgeDevicesCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDevicesCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingEdgeDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetEdgeDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedEdgeDevicesCountCountShouldReturnConnectedEdgeDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND connectionState = 'Connected'")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetConnectedEdgeDevicesCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedEdgeDevicesCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingConnectedEdgeDevicesCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND connectionState = 'Connected'")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetConnectedEdgeDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConcentratorsCountShouldReturnConcentratorsCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND devices.tags.deviceType = 'LoRa Concentrator'")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetConcentratorsCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConcentratorsCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingConcentratorsCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND devices.tags.deviceType = 'LoRa Concentrator'")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetConcentratorsCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedConcentratorsCountShouldReturnConnectedConcentratorsCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ totalNumber: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND devices.tags.deviceType = 'LoRa Concentrator' AND connectionState = 'Connected'")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetConnectedConcentratorsCount();

            // Assert
            _ = result.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConnectedConcentratorsCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingConnectedConcentratorsCount()
        {
            // Arrange
            var service = CreateService();

            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new Exception("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                    It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = false AND devices.tags.deviceType = 'LoRa Concentrator' AND connectionState = 'Connected'")))
                .Returns(mockCountQuery.Object);

            // Act
            var act = () => service.GetConnectedConcentratorsCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }
    }
}
