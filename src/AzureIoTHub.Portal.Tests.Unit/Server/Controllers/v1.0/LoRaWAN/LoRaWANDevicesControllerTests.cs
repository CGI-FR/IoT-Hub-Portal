// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class LoRaWANDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANDevicesController>> mockLogger;
        private Mock<ILoRaWANCommandService> mockLoRaWANCommandService;
        private Mock<IDeviceService<LoRaDeviceDetails>> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDevicesController>>();
            this.mockLoRaWANCommandService = this.mockRepository.Create<ILoRaWANCommandService>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService<LoRaDeviceDetails>>();
        }

        private LoRaWANDevicesController CreateLoRaWANDevicesController()
        {
            return new LoRaWANDevicesController(
                this.mockLogger.Object,
                this.mockLoRaWANCommandService.Object,
                this.mockDeviceService.Object);
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
                    It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedPaginatedDevices);

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
    }
}
