using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class DeviceModelCommandsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockCommandsTableClient;
        private Mock<ILogger<DeviceModelCommandsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelCommandsController>>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
        }

        private DeviceModelCommandsController CreateDeviceModelCommandsController()
        {
            return new DeviceModelCommandsController(
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

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(mockCommandsTableClient.Object);

            // Act
            var result = await deviceModelCommandsController.Post(entity.RowKey, command);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once());
            this.mockDeviceTemplatesTableClient.Verify(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == DeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == entity.RowKey),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()), Times.Once);

            this.mockTableClientFactory.Verify(c => c.GetDeviceCommands(), Times.Once());
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
            var result = await deviceModelCommandsController.Post(Guid.NewGuid().ToString(), new DeviceModelCommand());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Delete_Should_Delete_Command()
        {
            // Arrange
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();
            var entity = this.SetupMockDeviceModel();
            string commandId = Guid.NewGuid().ToString();
            var mockResponse = this.mockRepository.Create<Response>();

            this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(x => x == entity.RowKey),
                It.Is<string>(x => x == commandId), 
                It.IsAny<ETag>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockTableClientFactory
                .Setup(c => c.GetDeviceCommands())
                .Returns(mockCommandsTableClient.Object);

            // Act
            var result = await deviceModelCommandsController.Delete(entity.RowKey, commandId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NoContentResult>(result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once());
            this.mockDeviceTemplatesTableClient.Verify(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == DeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == entity.RowKey),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()), Times.Once);

            this.mockCommandsTableClient.Verify(c => c.DeleteEntityAsync(
                It.Is<string>(x => x == entity.RowKey),
                It.Is<string>(x => x == commandId),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task When_Model_NotExists_Delete_Should_Return_404()
        {
            // Arrange
            var deviceModelCommandsController = this.CreateDeviceModelCommandsController();

            SetupNotFoundDeviceModel();

            // Act
            var result = await deviceModelCommandsController.Delete(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        private TableEntity SetupMockDeviceModel()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(DeviceModelsController.DefaultPartitionKey, modelId);

            mockResponse.Setup(c => c.Value)
                .Returns(entity);

            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == DeviceModelsController.DefaultPartitionKey),
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
                    It.Is<string>(p => p == DeviceModelsController.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);
        }
    }
}
