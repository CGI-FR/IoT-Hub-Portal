// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<EdgeDevicesController>> mockLogger;
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IEdgeDevicesService> mockEdgeDeviceService;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IExternalDeviceService>();
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

            var expectedPaginedEdgeDevice = new PaginatedResult<IoTEdgeListItem>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new IoTEdgeListItem()
                {
                    DeviceId = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevicesPage(
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync(expectedPaginedEdgeDevice);

            var locationUrl = "http://location/edge/devices";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var result = await edgeDeviceController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPaginedEdgeDevice.Data.Count, result.Items.Count());

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
        public async Task WhenDeviceDoesNotExistGetEdgeDeviceSpecifyingIdShouldThrowAnException()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevice(It.IsAny<string>()))
                .ThrowsAsync(new DeviceNotFoundException(""));

            // Act
            var result = await edgeDeviceController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result);

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
                .ReturnsAsync(edgeDevice);

            // Act
            var result = await edgeDeviceController.CreateEdgeDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<IoTEdgeDevice>(okObjectResult.Value);

            var edgeDeviceObject = okObjectResult.Value as IoTEdgeDevice;
            Assert.IsNotNull(edgeDeviceObject);
            Assert.AreEqual(edgeDevice.DeviceId, edgeDeviceObject.DeviceId);

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
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.UpdateEdgeDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(edgeDevice);

            // Act
            var result = await edgeDeviceController.UpdateDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService.Setup(c => c.DeleteEdgeDeviceAsync(It.Is<string>(x => x == deviceId)))
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

        [Test]
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService
                .Setup(x => x.GetEdgeDeviceCredentials(It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new EnrollmentCredentials());

            // Act
            var result = await edgeDevicesController.GetCredentials(deviceId);

            // Assert
            Assert.IsNotNull(result);

            var okObjectResult = result.Result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<EnrollmentCredentials>(okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesNotExistGetEnrollmentCredentialsShouldThrowAnException()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService
                .Setup(x => x.GetEdgeDeviceCredentials(It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ResourceNotFoundException(""));

            // Act
            var result = await edgeDevicesController.GetCredentials(deviceId);

            // Assert
            Assert.IsNotNull(result);

            var objectResult = result.Result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(404, objectResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [TestCase("RestartModule")]
        public async Task ExecuteMethodShouldExecuteC2DMethod(string methodName)
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var module = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.ExecuteModuleMethod(It.IsAny<string>(),
                It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal)),
                It.Is<string>(c => c.Equals(methodName, StringComparison.Ordinal))))
                .ReturnsAsync(new C2Dresult());

            // Act
            var result = await edgeDeviceController.ExecuteModuleMethod(module.ModuleName, deviceId, methodName);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [TestCase("test")]
        public async Task ExecuteCustomModuleMethodShouldExecuteC2DMethod(string commandName)
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId =  Guid.NewGuid().ToString();
            var moduleName = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.ExecuteModuleMethod(
                    It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal)),
                    It.Is<string>(c => c.Equals(moduleName, StringComparison.Ordinal)),
                    It.Is<string>(c => c.Equals(commandName, StringComparison.Ordinal))))
                .ReturnsAsync(new C2Dresult());

            // Act
            var result = await edgeDeviceController.ExecuteModuleMethod(deviceId, moduleName, commandName);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvailableLabels_ExistingLabels_ReturnsLabels()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var expectedLabels = new List<LabelDto>()
            {
                new LabelDto()
            };

            _ = this.mockEdgeDeviceService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(expectedLabels);

            // Act
            var result = await edgeDeviceController.GetAvailableLabels();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLabels);
            this.mockRepository.VerifyAll();
        }
    }
}
