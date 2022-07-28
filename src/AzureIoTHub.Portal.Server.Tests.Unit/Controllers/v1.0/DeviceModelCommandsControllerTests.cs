// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Server.Exceptions;

    [TestFixture]
    public class DeviceModelCommandsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockCommandsTableClient;
        private Mock<ILogger<LoRaWANCommandsController>> mockLogger;
        private Mock<ILoRaWANCommandService> mockLoRaWANCommandService ;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANCommandsController>>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
            this.mockLoRaWANCommandService = this.mockRepository.Create<ILoRaWANCommandService>();
        }

        //[Test]
        //public async Task PostShouldCreateCommand()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();
        //    var entity = SetupMockDeviceModel();

        //    var command = new DeviceModelCommand
        //    {
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    var mockResponse = this.mockRepository.Create<Response>();

        //    _ = this.mockDeviceModelCommandMapper.Setup(c => c.UpdateTableEntity(
        //            It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
        //            It.Is<DeviceModelCommand>(x => x == command)));

        //    _ = this.mockCommandsTableClient.Setup(c => c.AddEntityAsync(
        //         It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
        //         It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //         It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
        //         It.IsAny<int?>(),
        //         It.IsAny<IEnumerable<string>>(),
        //         It.IsAny<CancellationToken>()))
        //        .Returns(Pageable<TableEntity>.FromPages(new[]
        //        {
        //            Page<TableEntity>.FromValues(new[]
        //            {
        //                new TableEntity(entity.RowKey, Guid.NewGuid().ToString())
        //            }, null, mockResponse.Object)
        //        }));

        //    _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
        //        It.Is<string>(x => x == entity.RowKey),
        //        It.IsAny<string>(),
        //        It.IsAny<ETag>(),
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    Act
        //   var result = await deviceModelCommandsController.Post(entity.RowKey, new[] { command });

        //    Assert
        //    Assert.IsNotNull(result);
        //    Assert.IsAssignableFrom<OkResult>(result);

        //    this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once());
        //    this.mockDeviceTemplatesTableClient.Verify(c => c.GetEntityAsync<TableEntity>(
        //                It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //                It.Is<string>(k => k == entity.RowKey),
        //                It.IsAny<IEnumerable<string>>(),
        //                It.IsAny<CancellationToken>()), Times.Once);

        //    this.mockTableClientFactory.Verify(c => c.GetDeviceCommands());
        //    this.mockDeviceModelCommandMapper.VerifyAll();
        //    this.mockCommandsTableClient.Verify(c => c.AddEntityAsync(
        //        It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
        //        It.IsAny<CancellationToken>()), Times.Once());
        //}

        //[Test]
        //public async Task PostShouldInternalServerErrorExceptionWhenQueryingExistingCommands()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
        //    var modelId = Guid.NewGuid().ToString();
        //    var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //            It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //            It.Is<string>(k => k == modelId),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);

        //    var command = new DeviceModelCommand
        //    {
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //            It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
        //            It.IsAny<int?>(),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .Throws(new RequestFailedException("test"));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    Act
        //   var act = () => deviceModelCommandsController.Post(entity.RowKey, new[] { command });

        //    Assert
        //   _ = await act.Should().ThrowAsync<InternalServerErrorException>();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public async Task PostShouldInternalServerErrorExceptionWhenDeletingExistingCommands()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    var mockResponseTableEntity = this.mockRepository.Create<Response<TableEntity>>();
        //    var mockResponse = this.mockRepository.Create<Response>();
        //    var modelId = Guid.NewGuid().ToString();
        //    var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //            It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //            It.Is<string>(k => k == modelId),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponseTableEntity.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);

        //    var command = new DeviceModelCommand
        //    {
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //            It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
        //            It.IsAny<int?>(),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .Returns(Pageable<TableEntity>.FromPages(new[]
        //        {
        //            Page<TableEntity>.FromValues(new[]
        //            {
        //                new TableEntity(entity.RowKey, Guid.NewGuid().ToString())
        //            }, null, mockResponse.Object)
        //        }));

        //    _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
        //            It.Is<string>(x => x == entity.RowKey),
        //            It.IsAny<string>(),
        //            It.IsAny<ETag>(),
        //            It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new RequestFailedException("test"));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    Act
        //   var act = () => deviceModelCommandsController.Post(entity.RowKey, new[] { command });

        //    Assert
        //   _ = await act.Should().ThrowAsync<InternalServerErrorException>();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public async Task PostShouldInternalServerErrorExceptionWhenAddingNewCommands()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    var mockResponseTableEntity = this.mockRepository.Create<Response<TableEntity>>();
        //    var mockResponse = this.mockRepository.Create<Response>();
        //    var modelId = Guid.NewGuid().ToString();
        //    var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

        //    var command = new DeviceModelCommand
        //    {
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    _ = this.mockDeviceModelCommandMapper.Setup(c => c.UpdateTableEntity(
        //        It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
        //        It.Is<DeviceModelCommand>(x => x == command)));

        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //            It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //            It.Is<string>(k => k == modelId),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponseTableEntity.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //            It.Is<string>(x => x == $"PartitionKey eq '{entity.RowKey}'"),
        //            It.IsAny<int?>(),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .Returns(Pageable<TableEntity>.FromPages(new[]
        //        {
        //            Page<TableEntity>.FromValues(new[]
        //            {
        //                new TableEntity(entity.RowKey, Guid.NewGuid().ToString())
        //            }, null, mockResponse.Object)
        //        }));

        //    _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
        //            It.Is<string>(x => x == entity.RowKey),
        //            It.IsAny<string>(),
        //            It.IsAny<ETag>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockCommandsTableClient.Setup(c => c.AddEntityAsync(
        //        It.Is<TableEntity>(x => x.RowKey == command.Name && x.PartitionKey == entity.RowKey),
        //        It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new RequestFailedException("test"));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    Act
        //   var act = () => deviceModelCommandsController.Post(entity.RowKey, new[] { command });

        //    Assert
        //   _ = await act.Should().ThrowAsync<InternalServerErrorException>();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public async Task WhenModelNotExistsPostShouldReturn404()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    SetupNotFoundDeviceModel();

        //    Act
        //   var result = await deviceModelCommandsController.Post(Guid.NewGuid().ToString(), new[] { new DeviceModelCommand() });

        //    Assert
        //    Assert.IsNotNull(result);
        //    Assert.IsAssignableFrom<NotFoundResult>(result);

        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public async Task WhenModelNotExistsGetShouldReturn404()
        //{
        //    Arrange
        //   var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    SetupNotFoundDeviceModel();

        //    Act
        //   var result = await deviceModelCommandsController.Get(Guid.NewGuid().ToString());

        //    Assert
        //    Assert.IsNotNull(result);
        //    Assert.IsAssignableFrom<NotFoundResult>(result.Result);

        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public async Task GetShouldReturnDeviceModelCommands()
        //{
        //    Arrange
        //   var deviceModel = SetupMockDeviceModel();
        //    var deviceModelId = deviceModel.RowKey;
        //    var deviceModelCommandsController = CreateDeviceModelCommandsController();
        //    var mockResponse = this.mockRepository.Create<Response>();

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //        It.Is<string>(x => x == $"PartitionKey eq '{deviceModelId}'"),
        //        It.IsAny<int?>(),
        //        It.IsAny<IEnumerable<string>>(),
        //        It.IsAny<CancellationToken>()))
        //        .Returns(Pageable<TableEntity>.FromPages(new[]
        //        {
        //            Page<TableEntity>.FromValues(new[]
        //            {
        //                new TableEntity(deviceModelId, Guid.NewGuid().ToString())
        //            }, null, mockResponse.Object)
        //        }));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    _ = this.mockDeviceModelCommandMapper.Setup(c => c.GetDeviceModelCommand(
        //        It.Is<TableEntity>(x => x.PartitionKey == deviceModelId)))
        //        .Returns(new DeviceModelCommand());

        //    Act
        //   var response = await deviceModelCommandsController.Get(deviceModelId);

        //    Assert
        //    Assert.IsNotNull(response);
        //    Assert.IsAssignableFrom<OkObjectResult>(response.Result);

        //    var okResult = (OkObjectResult)response.Result;

        //    Assert.IsNotNull(okResult);
        //    Assert.IsAssignableFrom<DeviceModelCommand[]>(okResult.Value);

        //    var result = (DeviceModelCommand[])okResult.Value;
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(1, result.Length);
        //}

        //[Test]
        //public async Task GetShouldThrowRequestFailedExceptionWhenIssueOccursWhenQueryingCommands()
        //{
        //    Arrange
        //   var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
        //    var deviceModelId = Guid.NewGuid().ToString();

        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //            It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //            It.Is<string>(k => k == deviceModelId),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);

        //    var deviceModelCommandsController = CreateDeviceModelCommandsController();

        //    _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
        //            It.Is<string>(x => x == $"PartitionKey eq '{deviceModelId}'"),
        //            It.IsAny<int?>(),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .Throws(new RequestFailedException("test"));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
        //        .Returns(this.mockCommandsTableClient.Object);

        //    Act
        //   var act = () => deviceModelCommandsController.Get(deviceModelId);

        //    Assert
        //   _ = await act.Should().ThrowAsync<InternalServerErrorException>();
        //    this.mockRepository.VerifyAll();
        //}

        //private TableEntity SetupMockDeviceModel()
        //{
        //    var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
        //    var modelId = Guid.NewGuid().ToString();
        //    var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

        //    _ = mockResponse.Setup(c => c.Value)
        //        .Returns(entity);

        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //                It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //                It.Is<string>(k => k == modelId),
        //                It.IsAny<IEnumerable<string>>(),
        //                It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockResponse.Object);

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);

        //    return entity;
        //}

        //private void SetupNotFoundDeviceModel()
        //{
        //    _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
        //            It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
        //            It.IsAny<string>(),
        //            It.IsAny<IEnumerable<string>>(),
        //            It.IsAny<CancellationToken>()))
        //        .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

        //    _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
        //        .Returns(this.mockDeviceTemplatesTableClient.Object);
        //}

        //private LoRaWANCommandsController CreateDeviceModelCommandsController()
        //{
        //    return new LoRaWANCommandsController(
        //        this.mockLogger.Object,
        //        this.mockDeviceModelCommandMapper.Object,
        //        this.mockTableClientFactory.Object,
        //        this.mockLoRaWANCommandService.Object);
        //}
    }
}
