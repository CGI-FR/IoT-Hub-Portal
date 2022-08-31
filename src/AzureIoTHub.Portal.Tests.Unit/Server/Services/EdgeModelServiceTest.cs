// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using Azure;
    using System.Collections.Generic;
    using System;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Moq;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Models.v10;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using System.IO;
    using FluentAssertions;
    using AzureIoTHub.Portal.Server.Exceptions;
    using Microsoft.Azure.Devices;
    using AzureIoTHub.Portal.Server.Entities;
    using System.Linq.Expressions;

    [TestFixture]
    public class EdgeModelServiceTest
    {
        private MockRepository mockRepository;

        private Mock<IConfigService> mockConfigService;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<IEdgeDeviceModelMapper> mockEdgeModelMapper;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<TableClient> mockEdgeDeviceTemplatesTableClient;
        private Mock<TableClient> mockEdgeModuleCommands;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigService = this.mockRepository.Create<IConfigService>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockEdgeModelMapper = this.mockRepository.Create<IEdgeDeviceModelMapper>();
            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
            this.mockEdgeDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockEdgeModuleCommands = this.mockRepository.Create<TableClient>();
        }

        public EdgeModelService CreateEdgeModelService()
        {
            return new EdgeModelService(
                this.mockConfigService.Object,
                this.mockTableClientFactory.Object,
                this.mockEdgeModelMapper.Object,
                this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void GetEdgeModelsShouldReturnAList()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var returnedIndex = 10;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockEdgeModelMapper.Setup(c => c.CreateEdgeDeviceModelListItem(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new IoTEdgeModelListItem());

            // Act
            var result = edgeModelService.GetEdgeModels();

            // Assert
            Assert.AreEqual(10, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenQueryFailedGetEdgeModelsShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = () => edgeModelService.GetEdgeModels();

            // Assert
            _ = result.Should().Throw<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeModelShouldReturnValue()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            _ = mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(Array.Empty<EdgeModuleCommand>(), null, mockModuleCommandResponse.Object)
            });

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
                .Returns(this.mockEdgeModuleCommands.Object);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockConfigService
                .Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(new List<IoTEdgeModule>());

            _ = this.mockEdgeModelMapper
                .Setup(x => x.CreateEdgeDeviceModel(It.IsAny<TableEntity>(), It.IsAny<List<IoTEdgeModule>>(), It.IsAny<IEnumerable<EdgeModuleCommand>>()))
                .Returns(new IoTEdgeModel());

            // Act
            var result = await edgeModelService.GetEdgeModel("test");

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void WhenGetEntityAsyncFailedGetEdgeModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.GetEdgeModel("test");

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404GetEdgeModelShouldThrowResourceNotFoundException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.GetEdgeModel("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeModelShouldCreateEdgeModelTemplate()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            SetupNotFoundEntity();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response>();

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(Array.Empty<EdgeModuleCommand>(), null, mockModuleCommandResponse.Object)
            });

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
                .Returns(this.mockEdgeModuleCommands.Object);

            _ = this.mockEdgeModelMapper
                .Setup(x => x.UpdateTableEntity(It.IsAny<TableEntity>(), It.IsAny<IoTEdgeModel>()));

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == edgeModel.ModelId && x.PartitionKey == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockConfigService
                .Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>())).Returns(Task.CompletedTask);

            // Act
            await edgeModelService.CreateEdgeModel(edgeModel);

            // Assert

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeModelAlreadyExistCreateEdgeModelShouldThrowResourceAlreadyExistsException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var newEdgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.CreateEdgeModel(newEdgeModel);

            // Assert
            _ = result.Should().ThrowAsync<ResourceAlreadyExistsException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityFailedCreateEdgeModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var newEdgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status400BadRequest, ""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.CreateEdgeModel(newEdgeModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenUpsertEntityAsyncFailedCreateEdgeModelShoulThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            SetupNotFoundEntity();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(Array.Empty<EdgeModuleCommand>(), null, mockModuleCommandResponse.Object)
            });

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
                .Returns(this.mockEdgeModuleCommands.Object);

            _ = this.mockEdgeModelMapper
                .Setup(x => x.UpdateTableEntity(It.IsAny<TableEntity>(), It.IsAny<IoTEdgeModel>()));

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == edgeModel.ModelId && x.PartitionKey == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.CreateEdgeModel(edgeModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeModelShouldUpdateEdgeModelTemplate()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(Array.Empty<EdgeModuleCommand>(), null, mockModuleCommandResponse.Object)
            });

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
                .Returns(this.mockEdgeModuleCommands.Object);

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            _ = mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            var mockResponseUpdate = this.mockRepository.Create<Response>();

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.GetEntityAsync<TableEntity>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(x => x.UpsertEntityAsync(It.IsAny<TableEntity>(), It.IsAny<TableUpdateMode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponseUpdate.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockEdgeModelMapper
                .Setup(x => x.UpdateTableEntity(It.IsAny<TableEntity>(), It.IsAny<IoTEdgeModel>()));

            _ = this.mockConfigService
                .Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>())).Returns(Task.CompletedTask);

            // Act
            await edgeModelService.UpdateEdgeModel(edgeModel);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeModelIdIsNullUpdateEdgeModelShouldThrowRequestFailedException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = null
            };

            // Act
            var result = async () => await edgeModelService.UpdateEdgeModel(edgeModel);

            // Assert
            _ = result.Should().ThrowAsync<RequestFailedException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityFailedUpdateEdgeModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var newEdgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status400BadRequest, ""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.UpdateEdgeModel(newEdgeModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenUpsertEntityAsyncFailedUpdateEdgeModelShoulThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(Array.Empty<EdgeModuleCommand>(), null, mockModuleCommandResponse.Object)
            });

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
                .Returns(this.mockEdgeModuleCommands.Object);

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            _ = mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.GetEntityAsync<TableEntity>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(x => x.UpsertEntityAsync(It.IsAny<TableEntity>(), It.IsAny<TableUpdateMode>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockEdgeModelMapper
                .Setup(x => x.UpdateTableEntity(It.IsAny<TableEntity>(), It.IsAny<IoTEdgeModel>()));

            // Act
            var result = async () => await edgeModelService.UpdateEdgeModel(edgeModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeModelShouldDeleteEdgeModelTemplate()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            var config = new Configuration(Guid.NewGuid().ToString());

            var listEdgeModels = new List<Configuration>()
            {
                config
            };

            var mockResponseUpdate = this.mockRepository.Create<Response>();

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(x => x.DeleteEntityAsync(It.IsAny<string>(), It.Is<string>(c => c.Equals(config.Id, StringComparison.Ordinal)), It.IsAny<ETag>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponseUpdate.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockConfigService
                .Setup(x => x.GetIoTEdgeConfigurations())
                .ReturnsAsync(listEdgeModels);

            _ = this.mockConfigService
                .Setup(x => x.DeleteConfiguration(It.Is<string>(c => c.Equals(config.Id, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            await edgeModelService.DeleteEdgeModel(config.Id);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeleteEntityAsyncFailedDeleteEdgeModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(x => x.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ETag>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var result = async () => await edgeModelService.DeleteEdgeModel(modelId);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeModelAvatarShouldReturnValue()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            var expectedUrl = new Uri($"http://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedUrl);

            // Act
            var response = await edgeModelService.GetEdgeModelAvatar(entity.RowKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedUrl, response);

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404StatusCodeGetEdgeModelAvatarShouldThrowResourceNotFoundException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, ""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);


            // Act
            var response = async () => await edgeModelService.GetEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedGetEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);


            // Act
            var response = async () => await edgeModelService.GetEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeModelAvatarShouldUpdateValue()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            var mockFile = new Mock<IFormFile>();
            var expectedUrl = new Uri($"http://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager
                .Setup(c => c.ChangeDeviceModelImageAsync(It.Is<string>(x => x == entity.RowKey), It.IsAny<Stream>()))
                .ReturnsAsync(expectedUrl.ToString());

            // Act
            var response = await edgeModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedUrl.ToString(), response);

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedUpdateEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var mockFile = new Mock<IFormFile>();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);


            // Act
            var response = async () => await edgeModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404StatusCodeUpdateEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var mockFile = new Mock<IFormFile>();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, ""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);


            // Act
            var response = async () => await edgeModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeModelAvatarShouldDeleteValue()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.DeleteDeviceModelImageAsync(It.Is<string>(c => c == entity.RowKey)))
                .Returns(Task.CompletedTask);

            // Act
            await edgeModelService.DeleteEdgeModelAvatar(entity.RowKey);

            // Assert

            this.mockDeviceModelImageManager.Verify(c => c.DeleteDeviceModelImageAsync(entity.RowKey), Times.Once);
            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404DeleteEdgeModelAvatarShouldThrowResourceNotFoundException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, ""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var response = async () => await edgeModelService.DeleteEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedDeleteEdgeModelAvatarShouldThrowResourceNotFoundException()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(""));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            // Act
            var response = async () => await edgeModelService.DeleteEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        private TableEntity SetupMockEntity()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            return entity;
        }

        private void SetupNotFoundEntity()
        {
            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);
        }
    }
}
