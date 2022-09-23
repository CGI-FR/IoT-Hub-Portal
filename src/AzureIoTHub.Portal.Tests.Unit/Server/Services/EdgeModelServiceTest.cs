// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeModelServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDeviceModelRepository> mockEdgeDeviceModelRepository;
        private Mock<IEdgeDeviceModelCommandRepository> mockEdgeDeviceModelCommandRepository;
        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        private IEdgeModelService edgeDeviceModelService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();

            this.mockEdgeDeviceModelCommandRepository = MockRepository.Create<IEdgeDeviceModelCommandRepository>();
            this.mockConfigService = MockRepository.Create<IConfigService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            //this.mockEdgeDeviceModelMapper = MockRepository.Create<IEdgeDeviceModelMapper>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton<IEdgeModelService, EdgeModelService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.edgeDeviceModelService = Services.GetRequiredService<IEdgeModelService>();

        }

        [Test]
        public void GetEdgeModelsShouldReturnAList()
        {
            // Arrange
            var expectedEdgeDeviceModels = Fixture.CreateMany<EdgeDeviceModel>(3).ToList();
            var expectedIoTEdgeDeviceModelListItems = expectedEdgeDeviceModels.Select(model => Mapper.Map<IoTEdgeModelListItem>(model)).ToList();
            var expectedImageUri = Fixture.Create<Uri>();

            foreach (var item in expectedIoTEdgeDeviceModelListItems)
            {
                item.ImageUrl = expectedImageUri;
            }

            _ = this.mockEdgeDeviceModelRepository.Setup(repo => repo.GetAll())
                .Returns(expectedEdgeDeviceModels);

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            // Act
            var result = this.edgeDeviceModelService.GetEdgeModels();


            // Assert
            _ = result.Should().BeEquivalentTo(expectedIoTEdgeDeviceModelListItems);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeModelShouldReturnValueAsync()
        {
            // Arrange
            var expectedModules = Fixture.CreateMany<IoTEdgeModule>(2).ToList();
            var expectedRoutes = Fixture.CreateMany<IoTEdgeRoute>(2).ToList();
            var expectedImageUri = Fixture.Create<Uri>();

            var expectedEdgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                ImageUrl = expectedImageUri,
                Description = Guid.NewGuid().ToString(),
                EdgeModules = expectedModules,
                EdgeRoutes = expectedRoutes
            };

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = expectedEdgeDeviceModel.ModelId;
                command.ModuleName = expectedModules[0].ModuleName;
                return command;
            }) .ToList();

            var expectedEdgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(expectedEdgeDeviceModel);

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedEdgeDeviceModelEntity);

            _ = this.mockConfigService.Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(expectedModules);
            _ = this.mockConfigService.Setup(x => x.GetConfigRouteList(It.IsAny<string>()))
                .ReturnsAsync(expectedRoutes);
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAll())
                .Returns(expectedCommands);

            // Act
            var result = await this.edgeDeviceModelService.GetEdgeModel(expectedEdgeDeviceModel.ModelId);

            // Assert
            Assert.IsNotNull(result);
            _ = result.Should().BeEquivalentTo(expectedEdgeDeviceModel);
        }

        [Test]
        public void GetEdgeModelShouldThrowResourceNotFoundExceptionIfModelDoesNotExist()
        {

            // Arrange
            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);

            // Act
            var result = async () => await this.edgeDeviceModelService.GetEdgeModel("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public async Task CreateEdgeModelShouldCreateEdgeModel()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDeviceModel>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAll())
                .Returns(expectedCommands);
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.InsertAsync(It.IsAny<EdgeDeviceModelCommand>()))
                 .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDeviceModelService.CreateEdgeModel(edgeDeviceModel);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public void CreateEdgeModelShouldThrowResourceAlreadyExistsExceptionIfModelAlreadyExists()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(edgeDeviceModelEntity);

            // Act
            var result = async () => await this.edgeDeviceModelService.CreateEdgeModel(edgeDeviceModel);

            // Assert
            _ = result.Should().ThrowAsync<ResourceAlreadyExistsException>();
        }

        [Test]
        public void CreateEdgeModelShouldThrowInternalServerErrorExceptionIfDbUpdateExceptionOccurs()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDeviceModel>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Throws(new DbUpdateException());

            // Act
            var result = async () => await this.edgeDeviceModelService.CreateEdgeModel(edgeDeviceModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
        }

        [Test]
        public async Task UpdateEdgeModelShouldUpdateEdgeModel()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(edgeDeviceModelEntity);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAll())
                .Returns(expectedCommands);
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.InsertAsync(It.IsAny<EdgeDeviceModelCommand>()))
                 .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDeviceModelService.UpdateEdgeModel(edgeDeviceModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void UpdateEdgeModelShouldThrowResourceNotFoundExceptionIfModelDoesNotExist()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);

            // Act
            var result = async () => await this.edgeDeviceModelService.UpdateEdgeModel(edgeDeviceModel);

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public void UpdateEdgeModelShouldThrowInternalServerErrorExceptionIfDbUpdateExceptionOccurs()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(edgeDeviceModelEntity);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Throws(new DbUpdateException());

            // Act
            var result = async () => await this.edgeDeviceModelService.UpdateEdgeModel(edgeDeviceModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
        }

        [Test]
        public async Task DeleteEdgeModelShouldDeleteEdgeModel()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAll())
                .Returns(expectedCommands);
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));

            var configurations = new List<Configuration>()
            {
                new Configuration(edgeDeviceModel.ModelId)
            };
            _ = this.mockConfigService.Setup(x => x.GetIoTEdgeConfigurations())
                .ReturnsAsync(configurations);
            _ = this.mockConfigService.Setup(x => x.DeleteConfiguration(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDeviceModelService.DeleteEdgeModel(edgeDeviceModel.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void DeleteEdgeModelShouldThrowInternalServerErrorExceptionIfDbUpdateExceptionOccurs()
        {
            // Arrange
            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Throws(new DbUpdateException());

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAll())
                .Returns(expectedCommands);
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));

            // Act
            var result = async () => await this.edgeDeviceModelService.DeleteEdgeModel(edgeDeviceModel.ModelId);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
        }

        /*
        [Test]
        public async Task GetEdgeModelAvatarShouldReturnValue()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            var expectedUrl = new Uri($"http://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedUrl);

            // Act
            var response = await this.edgeDeviceModelService.GetEdgeModelAvatar(entity.RowKey);

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
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.GetEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedGetEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.GetEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeModelAvatarShouldUpdateValue()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            var mockFile = new Mock<IFormFile>();
            var expectedUrl = new Uri($"http://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager
                .Setup(c => c.ChangeDeviceModelImageAsync(It.Is<string>(x => x == entity.RowKey), It.IsAny<Stream>()))
                .ReturnsAsync(expectedUrl.ToString());

            // Act
            var response = await this.edgeDeviceModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedUrl.ToString(), response);

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SaveModuleCommandsShouldUpsertModuleCommandTemplates()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

            var modules = new List<IoTEdgeModule>()
            {
                new IoTEdgeModule
                {
                    ModuleName = "Test",
                    Commands = new List<IoTEdgeModuleCommand>
                    {
                        new IoTEdgeModuleCommand
                        {
                            Name = "Command"
                        }
                    }
                }
            };

            var iotEdgeModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString(),
                EdgeModules = modules,
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

            _ = this.mockEdgeModuleCommands
                   .Setup(c => c.UpsertEntity(
                    It.Is<EdgeModuleCommand>(x => x.RowKey == "Test-Command" && x.PartitionKey == iotEdgeModel.ModelId),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
               .Returns((Response)null);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
               .Returns(this.mockEdgeModuleCommands.Object);

            // Act
            await this.edgeDeviceModelService.SaveModuleCommands(iotEdgeModel);

            // Assert
            this.mockEdgeModuleCommands.Verify(c => c.UpsertEntity(It.IsAny<EdgeModuleCommand>(), TableUpdateMode.Merge, default), Times.Once);
            this.mockTableClientFactory.Verify(c => c.GetEdgeModuleCommands(), Times.Exactly(2));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SaveModuleCommandsShouldDeleteModuleCommandTemplates()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

            var modules = new List<IoTEdgeModule>()
            {
                new IoTEdgeModule
                {
                    ModuleName = "Test",
                    Commands = new List<IoTEdgeModuleCommand>()
                }
            };

            var iotEdgeModel = new IoTEdgeModel
            {
                ModelId = Guid.NewGuid().ToString(),
                EdgeModules = modules,
            };

            var mockModuleCommandResponse = this.mockRepository.Create<Response>();

            var mockModuleCommands = Pageable<EdgeModuleCommand>.FromPages(new[]
            {
                Page<EdgeModuleCommand>.FromValues(new EdgeModuleCommand[1]
                {
                    new EdgeModuleCommand
                    {
                        PartitionKey = iotEdgeModel.ModelId,
                        RowKey = "Test-Command",
                        Timestamp = DateTime.Now,
                        Name = "Command",
                    }
                }, null, mockModuleCommandResponse.Object)
            });

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockEdgeModuleCommands.Setup(c => c.Query(
                        It.IsAny<Expression<Func<EdgeModuleCommand, bool>>>(),
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(mockModuleCommands);

            _ = this.mockEdgeModuleCommands
                   .Setup(c => c.DeleteEntityAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ETag>(),
                    It.IsAny<CancellationToken>()))
               .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeModuleCommands())
               .Returns(this.mockEdgeModuleCommands.Object);

            // Act
            await this.edgeDeviceModelService.SaveModuleCommands(iotEdgeModel);

            // Assert
            this.mockEdgeModuleCommands.Verify(c => c.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), default, default), Times.Once);
            this.mockTableClientFactory.Verify(c => c.GetEdgeModuleCommands(), Times.Exactly(2));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedUpdateEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404StatusCodeUpdateEdgeModelAvatarShouldThrowInternalServerErrorException()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.UpdateEdgeModelAvatar(entity.RowKey, mockFile.Object);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeModelAvatarShouldDeleteValue()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.DeleteDeviceModelImageAsync(It.Is<string>(c => c == entity.RowKey)))
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDeviceModelService.DeleteEdgeModelAvatar(entity.RowKey);

            // Assert

            this.mockDeviceModelImageManager.Verify(c => c.DeleteDeviceModelImageAsync(entity.RowKey), Times.Once);
            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedWith404DeleteEdgeModelAvatarShouldThrowResourceNotFoundException()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.DeleteEdgeModelAvatar(entity.RowKey);

            // Assert
            _ = response.Should().ThrowAsync<ResourceNotFoundException>();

            this.mockTableClientFactory.Verify(c => c.GetEdgeDeviceTemplates(), Times.Once);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetEntityAsyncFailedDeleteEdgeModelAvatarShouldThrowResourceNotFoundException()
        {
            // Arrange
            //var this.edgeDeviceModelService = CreateEdgeModelService();

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
            var response = async () => await this.edgeDeviceModelService.DeleteEdgeModelAvatar(entity.RowKey);

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
    }*/
    }
}
