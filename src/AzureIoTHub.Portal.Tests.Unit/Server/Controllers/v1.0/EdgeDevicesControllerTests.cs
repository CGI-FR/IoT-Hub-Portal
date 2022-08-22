// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models.v10;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.Devices;

    [TestFixture]
    public class EdgeDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<EdgeDevicesController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IEdgeDevicesService> mockEdgeDeviceService;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockEdgeDeviceService = this.mockRepository.Create<IEdgeDevicesService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
        }

        private EdgeDevicesController CreateEdgeDevicesController()
        {
            return new EdgeDevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockEdgeDeviceService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllDeviceShouldReturnOkResult()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceTwinListPage = new PaginationResult<Twin>();

            _ = this.mockDeviceService
                .Setup(x => x.GetAllEdgeDevice(It.Is<string>(c => c.Equals("aaa", StringComparison.Ordinal)),
                It.Is<string>(c => c.Equals("bbb", StringComparison.Ordinal)),
                It.Is<bool>(c => c.Equals(true)), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(deviceTwinListPage);

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevicesPage(deviceTwinListPage, It.IsAny<IUrlHelper>(),
                It.Is<string>(c => c.Equals("bbb", StringComparison.Ordinal)), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new PaginationResult<IoTEdgeListItem>()
                {
                    Items = Enumerable.Range(0, 10).Select(x => new IoTEdgeListItem()
                    {
                        DeviceId = FormattableString.Invariant($"{x}"),
                    }),
                    NextPage = Guid.NewGuid().ToString(),
                    TotalItems = 100
                });

            // Act
            var result = await edgeDeviceController.Get(continuationToken: "aaa",
                searchText: "bbb",
                searchStatus: true,
                pageSize: 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);

            Assert.IsAssignableFrom<PaginationResult<IoTEdgeListItem>>(okObjectResult.Value);

            var paginationResult = okObjectResult.Value as PaginationResult<IoTEdgeListItem>;

            Assert.IsNotNull(paginationResult);
            Assert.AreEqual(10, paginationResult.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSpecifyingIdGetShouldReturnTheEdgeDevice()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevice(It.IsAny<string>()))
                .ReturnsAsync(new IoTEdgeDevice()
                {
                    DeviceId = deviceId,
                });

            // Act
            var result = await edgeDeviceController.Get(deviceId);

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
        public async Task CreateEdgeDeviceAsyncShouldReturnOk()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.CreateEdgeDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new BulkRegistryOperationResult() { IsSuccessful = true });

            // Act
            var result = await edgeDeviceController.CreateEdgeDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<BulkRegistryOperationResult>(okObjectResult.Value);

            var bulkOperationResult = okObjectResult.Value as BulkRegistryOperationResult;
            Assert.IsNotNull(bulkOperationResult);
            Assert.AreEqual(true, bulkOperationResult.IsSuccessful);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncShouldReturnOkResult()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Environment = "fake"
            };

            var deviceTwin = new Twin(edgeDevice.DeviceId);
            deviceTwin.Tags["env"] = "dev";

            _ = this.mockEdgeDeviceService
                .Setup(x => x.UpdateEdgeDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(deviceTwin);

            // Act
            var result = await edgeDeviceController.UpdateDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);

            Assert.IsAssignableFrom<Twin>(okObjectResult.Value);
            Assert.AreEqual(deviceTwin, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.DeleteDevice(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await edgeDevicesController.DeleteDeviceAsync(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnLogsWhenNoErrorIsReturned()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(c => c.GetEdgeDeviceLogs(
                It.Is<string>(x => x == deviceId),
                It.Is<IoTEdgeModule>(x => x == edgeModule)))
                .ReturnsAsync(new List<IoTEdgeDeviceLog>
                {
                    new IoTEdgeDeviceLog
                    {
                        Id = deviceId,
                        Text = Guid.NewGuid().ToString(),
                        LogLevel = 1,
                        TimeStamp = DateTime.UtcNow
                    }
                });

            // Act
            var result = await edgeDevicesController.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Count().Should().Be(1);
        }

        [Test]
        public async Task GetEdgeDeviceLogsThrowsArgumentNullExceptionWhenModelIsNull()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            // Act
            var act = () => edgeDevicesController.GetEdgeDeviceLogs(deviceId, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
