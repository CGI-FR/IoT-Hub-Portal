// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN.Concentrator;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using static AzureIoTHub.Portal.Server.Startup;

    [TestFixture]
    public class LoRaWANConcentratorsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANConcentratorsController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IRouterConfigManager> mockRouterConfigManager;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;
        private Mock<ConfigHandler> mockConfigHandler;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANConcentratorsController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockRouterConfigManager = this.mockRepository.Create<IRouterConfigManager>();
            this.mockConcentratorTwinMapper = this.mockRepository.Create<IConcentratorTwinMapper>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
        }

        private LoRaWANConcentratorsController CreateLoRaWANConcentratorsController()
        {
            return new LoRaWANConcentratorsController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockRouterConfigManager.Object,
                this.mockConcentratorTwinMapper.Object)
            {
                Url = this.mockUrlHelper.Object
            }; ;
        }

        [Test]
        public async Task GetAllDeviceConcentratorWithDeviceTypeShouldReturnNotEmptyList()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();
            var count = 100;
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
                .ReturnsAsync(new Shared.PaginationResult<Twin>
                {
                    Items = Enumerable.Range(0, 100).Select(x => new Twin
                    {
                        DeviceId = FormattableString.Invariant($"{x}"),
                        Tags = twinCollection
                    }),
                    NextPage = Guid.NewGuid().ToString()
                });

            _ = this.mockConcentratorTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>()))
                .Returns<Twin>(x => new Concentrator
                {
                    DeviceId = x.DeviceId
                });

            _ = this.mockUrlHelper.Setup(c => c.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(Guid.NewGuid().ToString());

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

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new Shared.PaginationResult<Twin>
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

            var concentratorController = this.CreateLoRaWANConcentratorsController();

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
        public async Task CreateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = this.CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true
            };

            var routerConfig = new RouterConfig();
            var mockResult = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };

            var twin = new Twin
            {
                DeviceId = concentrator.DeviceId,
            };

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == twin.DeviceId),
                It.Is<bool>(x => !x),
                It.Is<Twin>(x => x.DeviceId == twin.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            _ = this.mockRouterConfigManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(routerConfig);

            _ = this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));
            _ = this.mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await concentratorController.CreateDeviceAsync(concentrator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<BulkRegistryOperationResult>(okObjectResult.Value);
            Assert.AreEqual(mockResult, okObjectResult.Value);
        }

        [Test]
        public async Task UpdateDeviceAsyncWithValidArgumentShouldReturnOkResult()
        {
            // Arrange
            var concentratorController = this.CreateLoRaWANConcentratorsController();
            var concentrator = new Concentrator
            {
                DeviceId = "4512457896451156",
                LoraRegion = Guid.NewGuid().ToString(),
                IsEnabled = true,
                RouterConfig = new RouterConfig()
            };

            var twin = new Twin
            {
                DeviceId = concentrator.DeviceId,
            };

            var device = new Device(concentrator.DeviceId);

            _ = this.mockRouterConfigManager.Setup(x => x.GetRouterConfig(It.Is<string>(c => c == concentrator.LoraRegion)))
                .ReturnsAsync(concentrator.RouterConfig);
            _ = this.mockConcentratorTwinMapper.Setup(x => x.UpdateTwin(It.Is<Twin>(c => c.DeviceId == twin.DeviceId), It.Is<Concentrator>(c => c.DeviceId == concentrator.DeviceId)));

            _ = this.mockDeviceService.Setup(x => x.GetDevice(It.Is<string>(c => c == concentrator.DeviceId)))
                .ReturnsAsync(device);
            _ = this.mockDeviceService.Setup(x => x.UpdateDevice(It.Is<Device>(c => c.Id == concentrator.DeviceId)))
                .ReturnsAsync(device);
            _ = this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == concentrator.DeviceId)))
                .ReturnsAsync(twin);
            _ = this.mockDeviceService.Setup(x => x.UpdateDeviceTwin(It.Is<string>(c => c == concentrator.DeviceId), It.Is<Twin>(c => c.DeviceId == concentrator.DeviceId)))
                .ReturnsAsync(twin);

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
            var concentratorController = this.CreateLoRaWANConcentratorsController();
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
