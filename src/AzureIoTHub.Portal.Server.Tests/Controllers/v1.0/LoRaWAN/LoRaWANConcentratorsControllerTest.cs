using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace AzureIoTHub.Portal.Server.Tests.Controllers.v10.LoRaWAN
{
    [TestFixture]
    public class LoRaWANConcentratorsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANConcentratorsController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IRouterConfigManager> mockRouterConfigManager;
        private Mock<IConcentratorTwinMapper> mockConcentratorTwinMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANConcentratorsController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockRouterConfigManager = this.mockRepository.Create<IRouterConfigManager>();
            this.mockConcentratorTwinMapper = this.mockRepository.Create<IConcentratorTwinMapper>();
        }

        private LoRaWANConcentratorsController CreateLoRaWANConcentratorsController()
        {
            return new LoRaWANConcentratorsController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockRouterConfigManager.Object,
                this.mockConcentratorTwinMapper.Object);
        }

        [Test]
        public async Task GetAllDeviceConcentrator_With_deviceType_Should_return_not_empty_list()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();
            int count = 100;
            TwinCollection twinCollection = new TwinCollection();
            twinCollection["deviceType"] = "LoRa Concentrator";

            this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(Enumerable.Range(0, 100).Select(x => new Twin
                {
                    DeviceId = x.ToString(),
                    Tags = twinCollection
                }));

            this.mockConcentratorTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>()))
                .Returns<Twin>(x => new Concentrator
                {
                    DeviceId = x.DeviceId
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
            var deviceList = okObjectResult.Value as IEnumerable<Concentrator>;

            Assert.IsNotNull(deviceList);
            Assert.AreEqual(count, deviceList.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeviceConcentrator_With_no_deviceType_Should_return_empty_list()
        {
            // Arrange
            var concentratorsController = CreateLoRaWANConcentratorsController();
            int count = 100;
            TwinCollection twinCollection = new TwinCollection();

            this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(Enumerable.Range(0, 0).Select(x => new Twin
                {
                    DeviceId = x.ToString()
                }));

            // Act
            var response = await concentratorsController.GetAllDeviceConcentrator().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(response.Result);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            // Assert.IsNull(okObjectResult.Value);
            var deviceList = okObjectResult.Value as IEnumerable<Concentrator>;

            Assert.IsNull(deviceList);
            // Assert.IsFalse(deviceList.Any());

            this.mockRepository.VerifyAll();
        }
    }
}
