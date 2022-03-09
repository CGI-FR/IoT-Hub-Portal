// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceProvisioningServiceManager> mockProvisioningServiceManager;
        private Mock<ILogger<DevicesController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, DeviceDetails>> mockDeviceTwinMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockLogger = this.mockRepository.Create<ILogger<DevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, DeviceDetails>>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
        }

        private DevicesController CreateDevicesController()
        {
            return new DevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTagService.Object,
                this.mockProvisioningServiceManager.Object,
                this.mockDeviceTwinMapper.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task GetListStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
            var count = 100;
            var twinCollection = new TwinCollection();
            twinCollection["deviceType"] = "test";

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.Is<string>(x => x == "LoRa Concentrator")))
                .ReturnsAsync(Enumerable.Range(0, 100).Select(x => new Twin
                {
                    DeviceId = FormattableString.Invariant($"{x}"),
                    Tags = twinCollection
                }));

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceListItem(It.IsAny<Twin>(),It.IsAny<IEnumerable<string>>()))
                .Returns<Twin,IEnumerable<string>>((x,y) => new DeviceListItem
                {
                    DeviceID = x.DeviceId
                });

            this.mockDeviceTagService.Setup(c => c.GetAllSearchableTagsNames())
                .Returns(new List<string>());

            // Act
            var result = await devicesController.GetItems();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
            var deviceID = "aaa";

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceID)))
                .Returns<string>(x => Task.FromResult(new Twin(x)));

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((x, y) => new DeviceDetails
                {
                    DeviceID = x.DeviceId
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
            var devicesController = this.CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa",
            };

            Twin twin = null;

            _ = this.mockDeviceTwinMapper.Setup(c => c.UpdateTwin(It.Is<Twin>(x => x.DeviceId == device.DeviceID), It.Is<DeviceDetails>(x => x == device)))
                .Callback<Twin, DeviceDetails>((t, d) => twin = t);

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
            var devicesController = this.CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa",
            };

            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = await devicesController.CreateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa"
            };
            var item = new Device(device.DeviceID);
            var twin = new Twin(device.DeviceID);

            _ = this.mockDeviceService.Setup(c => c.GetDevice(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(item);
            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == device.DeviceID)))
                .ReturnsAsync(twin);
            _ = this.mockDeviceService.Setup(c => c.UpdateDevice(It.Is<Device>(x => x.Id == item.Id)))
                .ReturnsAsync(item);
            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(It.Is<string>(x => x == device.DeviceID), It.Is<Twin>(x => x.DeviceId == device.DeviceID)))
                .ReturnsAsync(twin);

            _ = this.mockDeviceTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(x => x == twin), It.Is<DeviceDetails>(x => x == device)));

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
            var devicesController = this.CreateDevicesController();
            var device = new DeviceDetails
            {
                DeviceID = "aaa"
            };

            devicesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var result = await devicesController.UpdateDeviceAsync(device);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteStateUnderTestExpectedBehavior()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
            var deviceID = "aaa";

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
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();
            var mockRegistrationCredentials = new EnrollmentCredentials
            {
                RegistrationID = "aaa",
                SymmetricKey = "dfhjkfdgh"
            };

            var mockTwin = new Twin("aaa");
            mockTwin.Tags["modelId"] = "bbb";

            var mockTableClient = this.mockRepository.Create<TableClient>();
            var mockDeviceModelEntity = new TableEntity
            {
                [nameof(DeviceModel.Name)] = "ccc"
            };
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();

            _ = mockResponse.SetupGet(c => c.Value)
                .Returns(mockDeviceModelEntity);

            _ = mockTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                It.Is<string>(x => x == "0"),
                It.Is<string>(x => x == "bbb"),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockProvisioningServiceManager.Setup(c => c.GetEnrollmentCredentialsAsync("aaa", "ccc"))
                .ReturnsAsync(mockRegistrationCredentials);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(mockTwin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockTableClient.Object);

            // Act
            var response = await devicesController.GetCredentials("aaa");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            var okObjectResult = (OkObjectResult)response.Result;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(mockRegistrationCredentials, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceTypePropertyNotExistGetEnrollmentCredentialsShouldReturnBadRequest()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();

            var mockTwin = new Twin("aaa");

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(mockTwin);

            // Act
            var response = await devicesController.GetCredentials("aaa");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceNotExistGetEnrollmentCredentialsShouldReturnNotFound()
        {
            // Arrange
            var devicesController = this.CreateDevicesController();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync((Twin)null);

            // Act
            var response = await devicesController.GetCredentials("aaa");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundObjectResult>(response.Result);

            this.mockRepository.VerifyAll();
        }
    }
}
