// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Microsoft.AspNetCore.Http;
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

        [Test]
        public async Task GetEdgeDeviceModelAvatarShouldReturnEdgeDeviceModelAvatar()
        {
            // Arrange
            var expectedImageUri = Fixture.Create<Uri>();
            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            // Act
            var result = await this.edgeDeviceModelService.GetEdgeModelAvatar(Guid.NewGuid().ToString());

            // Assert
            _ = result.Should().Be(expectedImageUri.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeDeviceModelAvatarShouldUpdateEdgeDeviceModelAvatar()
        {
            // Arrange
            var expectedImageUri = Fixture.Create<Uri>();

            var mockFormFile = MockRepository.Create<IFormFile>();

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.ChangeDeviceModelImageAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(expectedImageUri.ToString());

            _ = mockFormFile.Setup(file => file.OpenReadStream())
                .Returns(Stream.Null);

            // Act
            var result = await this.edgeDeviceModelService.UpdateEdgeModelAvatar(Guid.NewGuid().ToString(), mockFormFile.Object);

            // Assert
            _ = result.Should().Be(expectedImageUri.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeDeviceModelAvatarShouldDeleteEdgeDeviceModelAvatar()
        {
            // Arrange
            _ = this.mockDeviceModelImageManager
                .Setup(manager => manager.DeleteDeviceModelImageAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDeviceModelService.DeleteEdgeModelAvatar(Guid.NewGuid().ToString());

            // Assert
            MockRepository.VerifyAll();
        }

        /*
        
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
    }*/
    }
}
