using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.v10.LoRaWAN
{
    [TestFixture]
    public class LoRaWANDevicesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANDevicesController>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<ILoraDeviceMethodManager> mockLoraDeviceMethodManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<TableClient> mockCommandsTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockLoraDeviceMethodManager = this.mockRepository.Create<ILoraDeviceMethodManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>(); 
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();

        }

        private LoRaWANDevicesController CreateLoRaWANDevicesController()
        {
            return new LoRaWANDevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTwinMapper.Object,
                this.mockTableClientFactory.Object,
                this.mockLoraDeviceMethodManager.Object,
                this.mockDeviceModelCommandMapper.Object);
        }

        [Test]
        public async Task ExecuteCommand_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var loRaWANDevicesController = this.CreateLoRaWANDevicesController();
            string deviceId = Guid.NewGuid().ToString();
            string commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{ commandId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(Guid.NewGuid().ToString(), commandId)
                                    }, null, mockResponse.Object)
                    }));

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));                

            // Act
            var result = await loRaWANDevicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommand_Failed_Should_Return_Http400()
        {
            // Arrange
            var loRaWANDevicesController = this.CreateLoRaWANDevicesController();
            string deviceId = Guid.NewGuid().ToString();
            string commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{ commandId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(Guid.NewGuid().ToString(), commandId)
                                    }, null, mockResponse.Object)
                    }));

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = await loRaWANDevicesController.ExecuteCommand(deviceId, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
            this.mockRepository.VerifyAll();
        }
    }
}
