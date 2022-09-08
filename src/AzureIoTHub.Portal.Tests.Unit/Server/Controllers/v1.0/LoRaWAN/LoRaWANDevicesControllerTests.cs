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
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANDevicesController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;
        private Mock<ILoRaWANDeviceService> mockLoRaWANDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();
            this.mockLoRaWANDeviceService = this.mockRepository.Create<ILoRaWANDeviceService>();
        }

        private LoRaWANDevicesController CreateLoRaWANDevicesController()
        {
            return new LoRaWANDevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTagService.Object,
                this.mockDeviceTwinMapper.Object,
                this.mockLoRaWANDeviceService.Object);
        }

        [Test]
        public async Task ExecuteCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaWANDevicesController = CreateLoRaWANDevicesController();
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            this.mockLoRaWANDeviceService.Setup(c => c.ExecuteLoRaWANCommand(deviceId, commandId))
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
            const int count = 100;
            var twinCollection = new TwinCollection();
            twinCollection["deviceType"] = "test";

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = Enumerable.Range(0, 100).Select(x => new Twin
                    {
                        DeviceId = FormattableString.Invariant($"{x}"),
                        Tags = twinCollection
                    })
                });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceListItem(It.IsAny<Twin>()))
                .Returns<Twin>((x) => new DeviceListItem
                {
                    DeviceID = x.DeviceId
                });

            _ = this.mockDeviceTagService.Setup(c => c.GetAllSearchableTagsNames())
                .Returns(new List<string>());

            // Act
            var result = await devicesController.SearchItems();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Items.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = CreateLoRaWANDevicesController();
            const string deviceID = "aaa";

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceID)))
                .Returns<string>(x => Task.FromResult(new Twin(x)));

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((x, _) => new LoRaDeviceDetails
                {
                    DeviceID = x.DeviceId,
                    AppEUI = Guid.NewGuid().ToString(),
                    AppKey = Guid.NewGuid().ToString(),
                    SensorDecoder = Guid.NewGuid().ToString(),
                });

            _ = this.mockDeviceTagService.Setup(c => c.GetAllTagsNames())
                .Returns(new List<string>());

            // Act
            var result = await devicesController.GetItem(deviceID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(deviceID, result.DeviceID);
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

            Twin twin = null;

            _ = this.mockDeviceService.Setup(c => c.GetDevice(It.IsAny<string>()))
                .ReturnsAsync((Device)null);

            _ = this.mockDeviceTwinMapper.Setup(c => c.UpdateTwin(It.Is<Twin>(x => x.DeviceId == device.DeviceID), It.Is<LoRaDeviceDetails>(x => x == device)))
                .Callback<Twin, LoRaDeviceDetails>((t, _) => twin = t);

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(It.Is<string>(x => x == device.DeviceID), It.Is<bool>(x => !x), It.Is<Twin>(x => x == twin), It.Is<DeviceStatus>(x => x == (device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled))))
                .ReturnsAsync(new BulkRegistryOperationResult { IsSuccessful = true });

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

            var item = new Device(device.DeviceID);
            var twin = new Twin(device.DeviceID);

            _ = this.mockDeviceService.Setup(c => c.GetDevice(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(item);
            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(twin);
            _ = this.mockDeviceService.Setup(c => c.UpdateDevice(It.Is<Device>(x => x.Id == item.Id)))
                .ReturnsAsync(item);
            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(It.Is<Twin>(x => x.DeviceId == device.DeviceID)))
                .ReturnsAsync(twin);

            _ = this.mockDeviceTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(x => x == twin), It.Is<LoRaDeviceDetails>(x => x == device)));

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

            _ = this.mockDeviceService.Setup(c => c.DeleteDevice(It.Is<string>(x => x == deviceID)))
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
