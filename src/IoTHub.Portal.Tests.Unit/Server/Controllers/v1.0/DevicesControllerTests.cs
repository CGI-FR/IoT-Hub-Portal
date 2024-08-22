// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Mappers;
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
    using Portal.Server.Controllers.v1._0;
    using Shared.Models.v1._0;

    [TestFixture]
    public class DevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IUrlHelper> mockUrlHelper;
        private Mock<ILogger<DevicesController>> mockLogger;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IDevicePropertyService> mockDevicePropertyService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, DeviceDetails>> mockDeviceTwinMapper;

        private Mock<IDeviceService<DeviceDetails>> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DevicesController>>();
            this.mockExternalDeviceService = this.mockRepository.Create<IExternalDeviceService>();
            this.mockDevicePropertyService = this.mockRepository.Create<IDevicePropertyService>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, DeviceDetails>>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService<DeviceDetails>>();
        }

        private DevicesController CreateDevicesController()
        {
            return new DevicesController(
                this.mockLogger.Object,
                this.mockDevicePropertyService.Object,
                this.mockDeviceService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetListStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateDevicesController();

            var mockTagSearch = new StringValues("test");

            var mockQueryCollection = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "tag.deviceType", mockTagSearch }
            });

            var mockHttpRequest = this.mockRepository.Create<HttpRequest>();
            var mockHttpContext = this.mockRepository.Create<HttpContext>();

            _ = mockHttpContext.SetupGet(c => c.Request)
                .Returns(mockHttpRequest.Object);

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
            var devicesController = CreateDevicesController();
            var device = new DeviceDetails
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
            var devicesController = CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa",
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
            var devicesController = CreateDevicesController();
            var device = new DeviceDetails
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
            var devicesController = CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa"
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
            var devicesController = CreateDevicesController();
            var device = new DeviceDetails
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
            var devicesController = CreateDevicesController();
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
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var devicesController = CreateDevicesController();
            const string deviceId = "aaa";
            var deviceDetails = new DeviceDetails
            {
                DeviceName = "aaa"
            };

            var expectedEnrollmentCredentials = new DeviceCredentials
            {
                AuthenticationMode = AuthenticationMode.SymmetricKey,
                SymmetricCredentials = new SymmetricCredentials
                {
                    RegistrationID = "aaa",
                    SymmetricKey = "dfhjkfdgh"
                }
            };

            _ = this.mockDeviceService.Setup(service => service.GetCredentials(deviceDetails))
                .ReturnsAsync(expectedEnrollmentCredentials);
            _ = this.mockDeviceService.Setup(service => service.GetDevice(deviceDetails.DeviceID))
                .ReturnsAsync(deviceDetails);

            // Act
            var response = await devicesController.GetCredentials(deviceDetails.DeviceID);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            var okObjectResult = (OkObjectResult)response.Result;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(expectedEnrollmentCredentials, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPropertiesShouldReturnDevicePropertyValues()
        {
            // Arrange
            var devicesController = CreateDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            var expectedDevicePropertyValues = new List<DevicePropertyValue>
            {
                new()
                {
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDevicePropertyService.Setup(c => c.GetProperties(deviceId))
                .ReturnsAsync(expectedDevicePropertyValues);

            // Act
            var result = await devicesController.GetProperties(deviceId);

            // Assert
            _ = result.Count().Should().Be(1);
            _ = result.Should().BeEquivalentTo(expectedDevicePropertyValues);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldSetDevicePropertyValues()
        {
            // Arrange
            var devicesController = CreateDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            var devicePropertyValues = new List<DevicePropertyValue>
            {
                new()
                {
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDevicePropertyService.Setup(c => c.SetProperties(deviceId, devicePropertyValues))
                .Returns(Task.CompletedTask);

            // Act
            var result = await devicesController.SetProperties(deviceId, devicePropertyValues);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result.Result);

            var okResult = (OkResult)result.Result;
            Assert.IsNotNull(okResult);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvailableLabels_ExistingLabels_ReturnsLabels()
        {
            // Arrange
            var devicesController = CreateDevicesController();

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
