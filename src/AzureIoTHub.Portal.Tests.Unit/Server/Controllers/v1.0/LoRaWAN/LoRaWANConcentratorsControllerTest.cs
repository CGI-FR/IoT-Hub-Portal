// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANConcentratorsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANConcentratorsController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;
        private Mock<IUrlHelper> mockUrlHelper;
        private Mock<ILoRaWANConcentratorService> mockLoRaWANConcentratorService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANConcentratorsController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockConcentratorTwinMapper = this.mockRepository.Create<IConcentratorTwinMapper>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockLoRaWANConcentratorService = this.mockRepository.Create<ILoRaWANConcentratorService>();
        }

        private LoRaWANConcentratorsController CreateLoRaWANConcentratorsController()
        {
            return new LoRaWANConcentratorsController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockConcentratorTwinMapper.Object,
                this.mockLoRaWANConcentratorService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllDeviceConcentratorWithDeviceTypeShouldReturnNotEmptyList()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();
            const int count = 100;
            var twinCollection = new TwinCollection();
            twinCollection["deviceType"] = "LoRa Concentrator";

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
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
                    }),
                    NextPage = Guid.NewGuid().ToString()
                });

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.GetAllDeviceConcentrator(It.IsAny<PaginationResult<Twin>>(), It.IsAny<IUrlHelper>()))
                .Returns((PaginationResult<Twin> r, IUrlHelper h) => new PaginationResult<Concentrator>
                {
                    Items = r.Items.Select(x => new Concentrator { DeviceId = x.DeviceId }),
                    NextPage = r.NextPage,
                    TotalItems = r.TotalItems
                });

            // Act
            var response = await concentratorsController.GetAllDeviceConcentrator().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(response.Result);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            var deviceList = okObjectResult.Value as PaginationResult<Concentrator>;

            Assert.IsNotNull(deviceList);
            Assert.AreEqual(count, deviceList.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeviceConcentratorWithNoDeviceTypeShouldReturnEmptyList()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.GetAllDeviceConcentrator(It.IsAny<PaginationResult<Twin>>(), It.IsAny<IUrlHelper>()))
                .Returns((PaginationResult<Twin> r, IUrlHelper h) => new PaginationResult<Concentrator>
                {
                    Items = r.Items.Select(x => new Concentrator { DeviceId = x.DeviceId }),
                    NextPage = r.NextPage,
                    TotalItems = r.TotalItems
                });

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = Enumerable.Range(0, 0).Select(x => new Twin(FormattableString.Invariant($"{x}")))
                });

            // Act
            var response = await concentratorsController.GetAllDeviceConcentrator().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(response.Result);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            var deviceList = okObjectResult.Value as IEnumerable<Concentrator>;

            Assert.IsNull(deviceList);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenGetAllDeviceThrowAnErrorGetAllDeviceConcentratorShouldThrowInternalserverErrorException()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new InternalServerErrorException(""));

            // Act
            var response = async () => await concentratorsController.GetAllDeviceConcentrator();

            // Assert
            _ = await response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceConcentratorWithValidArgumentShouldReturnConcentrator()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["deviceType"] = "LoRa Concentrator";
            var concentrator = new Concentrator
            {
                DeviceId = twin.DeviceId,
                DeviceType = "LoRa Concentrator"
            };

            _ = this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
                .ReturnsAsync(twin);

            _ = this.mockConcentratorTwinMapper.Setup(x => x.CreateDeviceDetails(It.Is<Twin>(c => c.DeviceId == twin.DeviceId)))
                .Returns(concentrator);

            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var response = await concentratorController.GetDeviceConcentrator(twin.DeviceId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as OkObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<Concentrator>(okObjectResult.Value);
            var device = okObjectResult.Value as Concentrator;
            Assert.IsNotNull(device);
            Assert.AreEqual(twin.DeviceId, device.DeviceId);
            Assert.AreEqual("LoRa Concentrator", device.DeviceType);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenGetDeviceTwinThrowAnErrorGetDeviceConcentratorShouldThrowInternalserverErrorException()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["deviceType"] = "LoRa Concentrator";

            _ = this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
                .ThrowsAsync(new InternalServerErrorException(""));

            var concentratorController = CreateLoRaWANConcentratorsController();

            // Act
            var response = async () => await concentratorController.GetDeviceConcentrator(twin.DeviceId);

            // Assert
            _ = await response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.CreateDeviceAsync(concentrator))
                .ReturnsAsync(true);

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            var okObjectResult = result as OkResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
        }

        [Test]
        public async Task WhenDeviceCreationFailedShouldReturnBadRequest()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.CreateDeviceAsync(concentrator))
                .ReturnsAsync(false);

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestResult>(result);
            var objectResult = result as BadRequestResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceAlreadyExistsShouldReturnBadRequestWithAltreadyExistsMessage()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            _ = this.mockLoRaWANConcentratorService.Setup(c => c.CreateDeviceAsync(concentrator))
                .Throws(new DeviceAlreadyExistsException(concentrator.DeviceId));

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            var objectResult = result as BadRequestObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            Assert.AreEqual($"Device {concentrator.DeviceId} already registered.", objectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true,
                RouterConfig = new RouterConfig()
            };

            _ = this.mockLoRaWANConcentratorService.Setup(x => x.UpdateDeviceAsync(It.Is<Concentrator>(c => c == concentrator)))
                .ReturnsAsync(true);

            // Act
            var result = await concentratorController.UpdateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
        }
        [Test]
        public async Task DeleteDeviceAsync()
        {
            // Arrange
            var concentratorController = CreateLoRaWANConcentratorsController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(x => x.DeleteDevice(It.Is<string>(c => c == deviceId)))
                .Returns(Task.CompletedTask);

            // Act
            _ = await concentratorController.Delete(deviceId);

            // Assert
            this.mockRepository.VerifyAll();
        }
    }
}
