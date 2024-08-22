// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using NUnit.Framework;
    using Portal.Server.Controllers.v1._0.LoRaWAN;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaWANDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANDevicesController>> mockLogger;
        private Mock<ILoRaWANCommandService> mockLoRaWANCommandService;
        private Mock<IDeviceService<LoRaDeviceDetails>> mockDeviceService;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDevicesController>>();
            this.mockLoRaWANCommandService = this.mockRepository.Create<ILoRaWANCommandService>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService<LoRaDeviceDetails>>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
        }

        private LoRaWANDevicesController CreateLoRaWANDevicesController()
        {
            var loRaGatewayIDList = new LoRaGatewayIDList
            {
                GatewayIds = new List<string>(){ "GatewayId1", "GatewayId2" }
            };

            return new LoRaWANDevicesController(
                this.mockLogger.Object,
                this.mockLoRaWANCommandService.Object,
                this.mockDeviceService.Object,
                loRaGatewayIDList)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task ExecuteCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaWANDevicesController = CreateLoRaWANDevicesController();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            this.mockLoRaWANCommandService.Setup(c => c.ExecuteLoRaWANCommand(deviceId, commandId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await loRaWANDevicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetListStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();

            var mockHttpRequest = this.mockRepository.Create<HttpRequest>();
            var mockHttpContext = this.mockRepository.Create<HttpContext>();

            _ = mockHttpContext.SetupGet(c => c.Request)
                .Returns(mockHttpRequest.Object);

            var mockQueryCollection = new QueryCollection(new Dictionary<string, StringValues>());

            _ = mockHttpRequest.Setup(c => c.Query)
                .Returns(mockQueryCollection);

            devicesController.ControllerContext = new ControllerContext(
                new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var expectedPaginatedDevices = new PaginatedResult<DeviceListItem>()
            {
                Data = Enumerable.Range(0, 100).Select(x => new DeviceListItem
                {
                    DeviceID = FormattableString.Invariant($"{x}")
                }).ToList(),
                TotalCount = 100
            };

            _ = this.mockDeviceService.Setup(service => service.GetDevices(It.IsAny<string>(), It.IsAny<bool?>(),
                    It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                    It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(expectedPaginatedDevices);

            var locationUrl = "http://location/devices";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var result = await devicesController.SearchItems();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPaginatedDevices.Data.Count, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();

            var device = new LoRaDeviceDetails
            {
                DeviceID = "aaa",
            };

            _ = this.mockDeviceService.Setup(service => service.GetDevice(device.DeviceID))
                .ReturnsAsync(device);

            // Act
            var result = await devicesController.GetItem(device.DeviceID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(device.DeviceID, result.DeviceID);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            var device = new LoRaDeviceDetails
            {
                DeviceID = "AF441BB83C90E946",
            };

            _ = this.mockDeviceService.Setup(service => service.CreateDevice(device))
                .ReturnsAsync(device);

            // Act
            var result = await devicesController.CreateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncModelNotValidShouldReturnBadRequest()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            var device = new LoRaDeviceDetails
            {
                DeviceID = "aaa",
            };

            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var act = () => devicesController.CreateDeviceAsync(device);

            // Assert
            _ = await act.Should().ThrowAsync<ProblemDetailsException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            var device = new LoRaDeviceDetails
            {
                DeviceID = "AF441BB83C90E946"
            };

            _ = this.mockDeviceService.Setup(service => service.UpdateDevice(device))
                .ReturnsAsync(device);

            // Act
            var result = await devicesController.UpdateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncModelNotValidShouldReturnBadRequest()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            var device = new LoRaDeviceDetails
            {
                DeviceID = "aaa"
            };

            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var act = () => devicesController.UpdateDeviceAsync(device);

            // Assert
            _ = await act.Should().ThrowAsync<ProblemDetailsException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            const string deviceID = "aaa";

            _ = this.mockDeviceService.Setup(service => service.DeleteDevice(deviceID))
                .Returns(Task.CompletedTask);

            // Act
            var result = await devicesController.Delete(deviceID);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEnrollmentCredentialsShouldAlwaysReturn404()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            const string deviceID = "aaa";

            // Act
            var response = await devicesController.GetCredentials(deviceID);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetGatewaysExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();

            var loRaGatewayIDList = new LoRaGatewayIDList
            {
                GatewayIds = new List<string>(){ "GatewayId1", "GatewayId2" }
            };

            // Act
            var result = devicesController.GetGateways();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var okObjectResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.IsNotNull(okObjectResult.Value);

            Assert.IsAssignableFrom<LoRaGatewayIDList>(okObjectResult.Value);
            _ = okObjectResult.Value.Should().BeEquivalentTo(loRaGatewayIDList);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTelemetry_ExistingDevice_ReturnsTelemetry()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            var expectedTelemetry = new List<LoRaDeviceTelemetryDto>()
            {
                new LoRaDeviceTelemetryDto()
            };

            _ = this.mockDeviceService.Setup(service => service.GetDeviceTelemetry(deviceId))
                .ReturnsAsync(expectedTelemetry);

            // Act
            var result = await devicesController.GetDeviceTelemetry(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTelemetry);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvailableLabels_ExistingLabels_ReturnsLabels()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();

            var expectedLabels = new List<LabelDto>()
            {
                new LabelDto()
            };

            _ = this.mockDeviceService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(expectedLabels);

            // Act
            var result = await devicesController.GetAvailableLabels();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLabels);
            this.mockRepository.VerifyAll();
        }
    }
}
