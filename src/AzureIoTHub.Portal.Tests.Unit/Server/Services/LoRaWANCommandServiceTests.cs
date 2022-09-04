// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Models.v10.LoRaWAN;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANCommandServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockCommandsTableClient;
        private Mock<ILogger<LoRaWANCommandService>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANCommandService>>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
        }

        [Test]
        public async Task PostShouldCreateCommand()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();
            var entity = SetupMockDeviceModel();

            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelCommandMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                    It.Is<DeviceModelCommand>(x => x == command)));

            _ = this.mockCommandsTableClient.Setup(c => c.AddEntityAsync(
                 It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                 It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                 It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
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

            _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(x => x == entity.RowKey),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            await deviceModelCommandsService.PostDeviceModelCommands(entity.RowKey, new[] { command });

            // Assert
            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once());
            this.mockDeviceTemplatesTableClient.Verify(c => c.GetEntityAsync<TableEntity>(
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
        public async Task PostShouldThrowInternalServerErrorExceptionWhenFailQueryingExistingCommands()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == modelId),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            var act = () => deviceModelCommandsService.PostDeviceModelCommands(entity.RowKey, new[] { command });

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldThrowInternalServerErrorExceptionWhenFailDeletingExistingCommands()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            var mockResponseTableEntity = this.mockRepository.Create<Response<TableEntity>>();
            var mockResponse = this.mockRepository.Create<Response>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == modelId),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponseTableEntity.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
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

            _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                    It.Is<string>(x => x == entity.RowKey),
                    It.IsAny<string>(),
                    It.IsAny<ETag>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            var act = () => deviceModelCommandsService.PostDeviceModelCommands(entity.RowKey, new[] { command });

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldThrowInternalServerErrorExceptionWhenFailAddingNewCommands()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            var mockResponseTableEntity = this.mockRepository.Create<Response<TableEntity>>();
            var mockResponse = this.mockRepository.Create<Response>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            var command = new DeviceModelCommand
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelCommandMapper.Setup(c => c.UpdateTableEntity(
                It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                It.Is<DeviceModelCommand>(x => x == command)));

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == modelId),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponseTableEntity.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
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

            _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                    It.Is<string>(x => x == entity.RowKey),
                    It.IsAny<string>(),
                    It.IsAny<ETag>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockCommandsTableClient.Setup(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            var act = () => deviceModelCommandsService.PostDeviceModelCommands(entity.RowKey, new[] { command });

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldThrowInternalServerErrorExceptionWhenModelDoesNotExist()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            SetupNotFoundDeviceModel();

            // Act
            var act = () => deviceModelCommandsService.PostDeviceModelCommands(Guid.NewGuid().ToString(), new[] { new DeviceModelCommand() });

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetShouldThrowInternalServerErrorExceptionWhenModelDoesNotExist()
        {
            // Arrange
            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            SetupNotFoundDeviceModel();

            // Act
            var act = () =>  deviceModelCommandsService.GetDeviceModelCommandsFromModel(Guid.NewGuid().ToString());

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetShouldReturnDeviceModelCommands()
        {
            // Arrange
            var deviceModel = SetupMockDeviceModel();
            var deviceModelId = deviceModel.RowKey;
            var deviceModelCommandsService = CreateDeviceModelCommandsService();
            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(x => x == $"PartitionKey eq '{deviceModelId}'"),
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

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            _ = this.mockDeviceModelCommandMapper.Setup(c => c.GetDeviceModelCommand(
                It.Is<TableEntity>(x => x.PartitionKey == deviceModelId)))
                .Returns(new DeviceModelCommand());

            // Act
            var result = await deviceModelCommandsService.GetDeviceModelCommandsFromModel(deviceModelId);

            // Assert
            Assert.IsAssignableFrom<DeviceModelCommand[]>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
        }

        [Test]
        public async Task GetShouldThrowRequestFailedExceptionWhenIssueOccursWhenQueryingCommands()
        {
            // Arrange
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var deviceModelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == deviceModelId),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            var deviceModelCommandsService = CreateDeviceModelCommandsService();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"PartitionKey eq '{deviceModelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            var act = () =>  deviceModelCommandsService.GetDeviceModelCommandsFromModel(deviceModelId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        private TableEntity SetupMockDeviceModel()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = mockResponse.Setup(c => c.Value)
                .Returns(entity);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            return entity;
        }

        private void SetupNotFoundDeviceModel()
        {
            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);
        }

        private LoRaWANCommandService CreateDeviceModelCommandsService()
        {
            return new LoRaWANCommandService(
                this.mockLogger.Object,
                this.mockDeviceModelCommandMapper.Object,
                this.mockTableClientFactory.Object);
        }
    }
}
