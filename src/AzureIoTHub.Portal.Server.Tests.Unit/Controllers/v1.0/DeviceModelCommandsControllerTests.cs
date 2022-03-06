using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    [TestFixture]
    public class DeviceModelCommandsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockCommandsTableClient;
        private Mock<ILogger<LoRaWANCommandsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANCommandsController>>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
        }

        private LoRaWANCommandsController CreateDeviceModelCommandsController()
        {
            return new LoRaWANCommandsController(
                this.mockLogger.Object,
                this.mockDeviceModelCommandMapper.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task Post_Should_Create_Command()
        {
            // Arrange
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();
            var entity = this.SetupMockDeviceModel();

            DeviceModelCommand command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response>();

            this.mockDeviceModelCommandMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                    It.Is<DeviceModelCommand>(x => x == command)));

            this.mockCommandsTableClient.Setup(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(x => x == $"PartitionKey eq '{ entity.RowKey }'"),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[]
                {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(entity.RowKey, Guid.NewGuid().ToString())
                    }, null, mockResponse.Object)
                }));

            this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(x => x == entity.RowKey),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(mockCommandsTableClient.Object);

            // Act
            var result = await deviceModelCommandsController.Post(entity.RowKey, new[] { command });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once());
            this.mockDeviceTemplatesTableClient.Verify(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == entity.RowKey),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()), Times.Once);

            this.mockTableClientFactory.Verify(c => c.GetDeviceCommands());
            this.mockDeviceModelCommandMapper.VerifyAll();
            this.mockCommandsTableClient.Verify(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task When_Model_NotExists_Post_Should_Return_404()
        {
            // Arrange
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();

            SetupNotFoundDeviceModel();

            // Act
            var result = await deviceModelCommandsController.Post(Guid.NewGuid().ToString(), new[] { new DeviceModelCommand() });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void When_Model_NotExists_Get_Should_Return_404()
        {
            // Arrange
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();

            SetupNotFoundDeviceModel();

            // Act
            var result = deviceModelCommandsController.Get(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void Get_Should_Return_Device_Model_Commands()
        {
            // Arrange
            var deviceModel = SetupMockDeviceModel();
            var deviceModelId = deviceModel.RowKey;
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();
            var mockResponse = this.mockRepository.Create<Response>();

            this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(x => x == $"PartitionKey eq '{ deviceModelId }'"),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[]
                {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(deviceModelId, Guid.NewGuid().ToString())
                    }, null, mockResponse.Object)
                }));

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(mockCommandsTableClient.Object);

            this.mockDeviceModelCommandMapper.Setup(c => c.GetDeviceModelCommand(
                It.Is<TableEntity>(x => x.PartitionKey == deviceModelId)))
                .Returns(new DeviceModelCommand {  });

            // Act
            var response = deviceModelCommandsController.Get(deviceModelId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            
            var okResult = (OkObjectResult)response.Result;

            Assert.IsNotNull(okResult);
            Assert.IsAssignableFrom<DeviceModelCommand[]>(okResult.Value);

            var result = (DeviceModelCommand[])okResult.Value;
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
        }

        private TableEntity SetupMockDeviceModel()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            mockResponse.Setup(c => c.Value)
                .Returns(entity);

            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            return entity;
        }

        private void SetupNotFoundDeviceModel()
        {
            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);
        }
    }
}
