// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using IoTHub.Portal.Application.Providers;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Shared.Constants;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using IoTHub.Portal.Domain.Entities;
    using Shared.Models.v1._0;
    using Device = Microsoft.Azure.Devices.Device;

    [TestFixture]
    public class ExternalDeviceServiceTests
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;
        private Mock<ServiceClient> mockServiceClient;
        private Mock<ILogger<ExternalDeviceService>> mockLogger;
        private Mock<IDeviceRegistryProvider> mockRegistryProvider;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;

        private AutoFixture.Fixture Fixture { get; } = new();

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockServiceClient = this.mockRepository.Create<ServiceClient>();
            this.mockLogger = this.mockRepository.Create<ILogger<ExternalDeviceService>>();
            this.mockRegistryProvider = this.mockRepository.Create<IDeviceRegistryProvider>();
            this.mockDeviceModelRepository = this.mockRepository.Create<IDeviceModelRepository>();
        }

        private ExternalDeviceService CreateService()
        {
            return new ExternalDeviceService(
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object,
                this.mockDeviceModelRepository.Object,
                this.mockRegistryProvider.Object);
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
        public async Task GetAllEdgeDevice_SearchStatusIsTrue_ReturnsEdgeDevicesWithEnabledStatus()
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
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = true AND status = 'enabled'"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND status = 'enabled'")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice(searchStatus: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDevice_SearchStatusIsFalse_ReturnsEdgeDevicesWithDisabledStatus()
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
                It.Is<string>(x => x == "SELECT * FROM devices WHERE devices.capabilities.iotEdge = true AND status = 'disabled'"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == "SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND status = 'disabled'")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice(searchStatus: false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDevice_SearchTextIsNotNull_ReturnsEdgeDevices()
        {
            // Arrange
            var service = CreateService();
            var mockQuery = this.mockRepository.Create<IQuery>();
            var searchText = Guid.NewGuid().ToString();

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
                It.Is<string>(x => x == $"SELECT * FROM devices WHERE devices.capabilities.iotEdge = true AND (STARTSWITH(deviceId, '{searchText}') OR (is_defined(tags.deviceName) AND STARTSWITH(tags.deviceName, '{searchText}')))"),
                It.Is<int>(x => x == 10)))
                .Returns(mockQuery.Object);

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true AND (STARTSWITH(deviceId, '{searchText}') OR (is_defined(tags.deviceName) AND STARTSWITH(tags.deviceName, '{searchText}')))")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice(searchText: searchText );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDevice_CountWithoutTotalNumber_ReturnsEmptyEdgeDevices()
        {
            // Arrange
            var service = CreateService();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ toto: 1}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllEdgeDevice_CountTotalNumberEqualZero_ReturnsEmptyEdgeDevices()
        {
            // Arrange
            var service = CreateService();
            var mockCountQuery = this.mockRepository.Create<IQuery>();

            _ = mockCountQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new string[]
                {
                    /*lang=json*/
                    "{ totalNumber: 0}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT COUNT() as totalNumber FROM devices WHERE devices.capabilities.iotEdge = true")))
                .Returns(mockCountQuery.Object);

            // Act
            var result = await service.GetAllEdgeDevice();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Items.Count());
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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

            // Act
            var act = () => service.GetDeviceTwinWithModule(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetDeviceTwinWhithModule_EqualZero_ReturnsEmptyDevices()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            // Act
            var result = () => service.GetDeviceTwinWithModule(deviceId);

            // Assert
            Assert.IsNotNull(result);
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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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

        [Test]
        public async Task GetDeviceTwinWithEdgeHubModuleStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new Twin[]
                {
                    new Twin(Guid.NewGuid().ToString())
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']"), It.Is<int>(x => x == 1)))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetDeviceTwinWithEdgeHubModule(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<Twin>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTwinWithEdgeHubModule()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']"), It.Is<int>(x => x == 1)))
                .Returns(mockQuery.Object);

            // Act
            var act = async () => await service.GetDeviceTwinWithEdgeHubModule(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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

            var twin = new Twin(deviceId)
            {
                ETag = Guid.NewGuid().ToString(),
            };

            _ = this.mockRegistryManager.Setup(c => c.UpdateTwinAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<Twin>(x => x == twin),
                It.Is<string>(x => x == twin.ETag)))
                .ReturnsAsync(new Twin());

            // Act
            var result = await service.UpdateDeviceTwin(twin);

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

            var twin = new Twin(deviceId)
            {
                ETag = Guid.NewGuid().ToString(),
            };

            _ = this.mockRegistryManager.Setup(c => c.UpdateTwinAsync(
                    It.Is<string>(x => x == deviceId),
                    It.Is<Twin>(x => x == twin),
                    It.Is<string>(x => x == twin.ETag)))
                .ThrowsAsync(new RequestFailedException("test"));

            // Act
            var act = () => service.UpdateDeviceTwin(twin);

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
                .ThrowsAsync(new RequestFailedException("test"));

            // Act
            var act = () => service.ExecuteC2DMethod(deviceId, method);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCustomCommandC2DMethodShouldReturn200()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var moduleName = Guid.NewGuid().ToString();

            var method = new CloudToDeviceMethod(Guid.NewGuid().ToString());

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)),
                It.Is<string>(x => x.Equals(moduleName, StringComparison.Ordinal)),
                It.Is<CloudToDeviceMethod>(x => x == method),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            // Act
            var result = await service.ExecuteCustomCommandC2DMethod(deviceId, moduleName, method);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.Status);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCustomCommandC2DMethodShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            var moduleName = Guid.NewGuid().ToString();

            var method = new CloudToDeviceMethod(Guid.NewGuid().ToString());

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)),
                It.Is<string>(x => x.Equals(moduleName, StringComparison.Ordinal)),
                It.Is<CloudToDeviceMethod>(x => x == method),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            // Act
            var result = async () => await service.ExecuteCustomCommandC2DMethod(deviceId, moduleName, method);

            // Assert
            _ = await result.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnLogsWhen200IsReturned()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = "1.0",
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


            var logger = Mock.Of<ILogger<ExternalDeviceService>>();

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == "$edgeAgent"),
                It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            var deviceService = new ExternalDeviceService(
                logger,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object,
                this.mockDeviceModelRepository.Object,
                this.mockRegistryProvider.Object);

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
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = "1.0",
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


            var logger = Mock.Of<ILogger<ExternalDeviceService>>();

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == "$edgeAgent"),
                It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 500
                });

            var deviceService = new ExternalDeviceService(
                logger,
                this.mockRegistryManager.Object,
                this.mockServiceClient.Object,
                this.mockDeviceModelRepository.Object,
                this.mockRegistryProvider.Object);

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
                ModuleName = Guid.NewGuid().ToString()
            };

            var method = new CloudToDeviceMethod(CloudToDeviceMethods.GetModuleLogs);

            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = "1.0",
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

            _ = this.mockServiceClient.Setup(c => c.InvokeDeviceMethodAsync(
                    It.Is<string>(x => x == deviceId),
                    It.Is<string>(x => x == "$edgeAgent"),
                    It.Is<CloudToDeviceMethod>(x => x.MethodName == method.MethodName && x.GetPayloadAsJson() == payload),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            var deviceService = CreateService();

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange

            var service = CreateService();
            var device = Fixture.Create<DeviceDetails>();
            var model = Fixture.Create<DeviceModel>();

            device.ModelId = model.Id;
            device.ModelName = model.Name;

            var mockRegistrationCredentials = new DeviceCredentials
            {
                AuthenticationMode = AuthenticationMode.SymmetricKey,
                SymmetricCredentials = new SymmetricCredentials
                {
                    RegistrationID = "aaa",
                    SymmetricKey = "dfhjkfdgh"
                }
            };

            _ = this.mockDeviceModelRepository.Setup(c => c.GetByIdAsync(device.ModelId))
                .ReturnsAsync(model);

            _ = this.mockRegistryProvider.Setup(c => c.GetEnrollmentCredentialsAsync(device.DeviceID, model.Id))
                .ReturnsAsync(mockRegistrationCredentials);

            // Act
            var enrollmentCredentials = await service.GetDeviceCredentials(device);

            // Assert
            _ = enrollmentCredentials.Should().BeEquivalentTo(mockRegistrationCredentials);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenUnableToGetModelGetEnrollmentCredentialsShouldThrowInternalServerErrorException()
        {
            // Arrange
            var service = CreateService();
            var device = Fixture.Create<DeviceDetails>();
            var model = Fixture.Create<DeviceModel>();

            device.ModelId = model.Id;

            _ = this.mockDeviceModelRepository.Setup(c => c.GetByIdAsync(model.Id))
                .ReturnsAsync(model);

            _ = this.mockRegistryProvider.Setup(c => c.GetEnrollmentCredentialsAsync(device.DeviceID, model.Id))
                .Throws(new RequestFailedException("test"));

            // Act
            var act = () => service.GetDeviceCredentials(device);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateNewTwinFromDeviceIdShouldReturnTwin()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            _ = this.mockRegistryManager.Setup(c => c.GetDeviceAsync(It.IsAny<string>()))
                .ReturnsAsync((Device)null);

            // Act
            var twin = await service.CreateNewTwinFromDeviceId(deviceId);

            // Assert
            Assert.AreEqual(deviceId, twin.DeviceId);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateNewTwinFromDeviceIdShouldThrowInternalErrorExceptionWhenDeviceAlreadyExists()
        {
            // Arrange
            var service = CreateService();
            var deviceId = Guid.NewGuid().ToString();
            _ = this.mockRegistryManager.Setup(c => c.GetDeviceAsync(It.IsAny<string>()))
                .ReturnsAsync(new Device());

            // Act
            var act = () => service.CreateNewTwinFromDeviceId(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<DeviceAlreadyExistsException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var service = CreateService();

            var fakeDevice = Fixture.Create<IoTEdgeDevice>();

            var mockRegistrationCredentials = Fixture.Create<DeviceCredentials>();

            var mockTwin = new Twin(fakeDevice.DeviceId);

            _ = this.mockRegistryManager.Setup(c => c.GetTwinAsync(It.Is<string>(x => x == mockTwin.DeviceId)))
                .ReturnsAsync(mockTwin);

            _ = this.mockRegistryProvider.Setup(c => c.GetEnrollmentCredentialsAsync(fakeDevice.DeviceId, It.IsAny<string>()))
                .ReturnsAsync(mockRegistrationCredentials);

            // Act
            var result = await service.GetEdgeDeviceCredentials(fakeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockRegistrationCredentials, result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceTwinIsNullGetEdgeDeviceCredentialsShouldThrowResourceNotFoundException()
        {
            // Arrange
            var service = CreateService();

            var fakeDevice = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockRegistryManager.Setup(c => c.GetTwinAsync(It.Is<string>(x => x == fakeDevice.DeviceId)))
                .ReturnsAsync(value: null);

            // Act

            // Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => service.GetEdgeDeviceCredentials(fakeDevice));

            this.mockRepository.VerifyAll();
        }


        [Test]
        public async Task GetAllGatewayIDExpectedBehaviorShouldReturnList()
        {
            // Arrange
            var service = CreateService();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new[]
                {
                    /*lang=json*/
                    "{ deviceId: 'value1'}",
                    /*lang=json*/
                    "{ deviceId: 'value2'}",
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT DeviceID FROM devices.modules WHERE devices.modules.moduleId = 'LoRaWanNetworkSrvModule'")))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetAllGatewayID();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<List<string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result[0]);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllGatewayIDIssueOccuringShouldThrowInternalServerErrorException()
        {
            // Arrange
            var service = CreateService();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT DeviceID FROM devices.modules WHERE devices.modules.moduleId = 'LoRaWanNetworkSrvModule'")))
                .Returns(mockQuery.Object);

            // Act
            var act = () => service.GetAllGatewayID();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesToExportExpectedBehaviorShouldReturnList()
        {
            // Arrange
            var service = CreateService();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsJsonAsync())
                .ReturnsAsync(new List<string>
                {
                    /*lang=json*/
                    "{ \"deviceId\": \"000001\", \"tags\": { \"deviceName\": \"DeviceExport01\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-1\"}, \"desired\": { \"Property1\": \"123\", \"Property2\": \"456\" }}",
                    /*lang=json*/
                    "{ \"deviceId\": \"000002\", \"tags\": { \"deviceName\": \"DeviceExport02\", \"supportLoRaFeatures\": \"true\", \"modelId\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Tag1\": \"Tag1-2\"}, \"desired\": { \"Property1\": \"789\", \"Property2\": \"000\" }}"
                });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT deviceId, tags, properties.desired FROM devices WHERE (NOT IS_DEFINED (tags.deviceType) OR tags.deviceType <> 'LoRa Concentrator') AND (capabilities.iotEdge = false)")))
                .Returns(mockQuery.Object);

            // Act
            var result = await service.GetDevicesToExport();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<List<string>>(result);
            Assert.AreEqual(2, result.Count());
            var deviceObjectResult = JsonNode.Parse(result.First())!;
            Assert.AreEqual("000001", deviceObjectResult["deviceId"].ToString());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesToExportIssueOccuringShouldThrowInternalServerErrorException()
        {
            // Arrange
            var service = CreateService();

            var mockQuery = this.mockRepository.Create<IQuery>();

            _ = mockQuery.Setup(c => c.GetNextAsJsonAsync())
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT deviceId, tags, properties.desired FROM devices WHERE (NOT IS_DEFINED (tags.deviceType) OR tags.deviceType <> 'LoRa Concentrator') AND (capabilities.iotEdge = false)")))
                .Returns(mockQuery.Object);

            // Act
            var act = () => service.GetDevicesToExport();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task RetrieveLastConfigurationTwinHasNoConfigurationShouldReturnNull()
        {
            // Arrange
            var service = CreateService();
            var mockDevice = Fixture.Create<IoTEdgeDevice>();
            _ = this.mockRegistryManager.Setup(c => c.GetTwinAsync(mockDevice.DeviceId)).ReturnsAsync(Fixture.Create<Twin>());
            // Act
            var result = await service.RetrieveLastConfiguration(mockDevice);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task CreateDeviceModel_AnyExternalDeviceModelDto_NotImplementedExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = new ExternalDeviceModelDto();
            var service = CreateService();

            // Act
            var act = () => service.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_AnyExternalDeviceModelDto_NotImplementedExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = new ExternalDeviceModelDto();
            var service = CreateService();

            // Act
            var act = () => service.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEnrollementScriptShouldThrowNotImplementedException()
        {
            // Arrange
            var template = Fixture.Create<string>();
            var edgeDevice = new IoTEdgeDevice();
            var service = CreateService();

            // Act
            var act = () => service.CreateEnrollementScript(template, edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task CreateEdgeDeviceShouldThrowNotImplementedException()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var service = CreateService();

            // Act
            var act = () => service.CreateEdgeDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task RemoveDeviceCredentialsShouldThrowNotImplementedException()
        {
            // Arrange
            var edgeDevice = new IoTEdgeDevice();
            var service = CreateService();

            // Act
            var act = () => service.RemoveDeviceCredentials(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task IsEdgeDeviceModelShouldThrowNotImplementedException()
        {
            // Arrange
            var externalDeviceModelDto = new ExternalDeviceModelDto();
            var service = CreateService();

            // Act
            var act = () => service.IsEdgeDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllThingShouldThrowNotImplementedException()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = () => service.GetAllThing();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }
    }
}
