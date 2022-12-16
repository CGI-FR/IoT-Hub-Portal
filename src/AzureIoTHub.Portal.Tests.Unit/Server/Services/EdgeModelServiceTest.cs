// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using AzureIoTHub.Portal.Shared.Models.v10;
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
        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IEdgeModuleCommandsService> mockEdgeModuleCommandsService;

        private IEdgeModelService edgeDeviceModelService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();

            this.mockConfigService = MockRepository.Create<IConfigService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockEdgeModuleCommandsService = MockRepository.Create<IEdgeModuleCommandsService>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeModuleCommandsService.Object);
            _ = ServiceCollection.AddSingleton<IEdgeModelService, EdgeModelService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.edgeDeviceModelService = Services.GetRequiredService<IEdgeModelService>();
            Mapper = Services.GetRequiredService<IMapper>();

        }

        [Test]
        public async Task GetEdgeModelsShouldReturnAList()
        {
            // Arrange
            var expectedEdgeDeviceModels = new List<EdgeDeviceModel>
            {
                new EdgeDeviceModel { Id = Guid.NewGuid().ToString() },
                new EdgeDeviceModel { Id = Guid.NewGuid().ToString() },
                new EdgeDeviceModel { Id = Guid.NewGuid().ToString() }
            };

            var expectedIoTEdgeDeviceModelListItems = expectedEdgeDeviceModels.Select(model => Mapper.Map<IoTEdgeModelListItem>(model)).ToList();
            var expectedImageUri = Fixture.Create<Uri>();

            foreach (var item in expectedIoTEdgeDeviceModelListItems)
            {
                item.ImageUrl = expectedImageUri;
            }

            var edgeModelFilter = new EdgeModelFilter
            {
                Keyword = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<EdgeDeviceModel, bool>>>(), default, new Expression<Func<EdgeDeviceModel, object>>[] { d => d.Labels }))
                .ReturnsAsync(expectedEdgeDeviceModels);

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            // Act
            var result = await this.edgeDeviceModelService.GetEdgeModels(edgeModelFilter);


            // Assert
            _ = result.Should().BeEquivalentTo(expectedIoTEdgeDeviceModelListItems);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeModelShouldReturnValueAsync()
        {
            // Arrange
            var expectedModules = new List<IoTEdgeModule>
            {
                new IoTEdgeModule(){ ModuleName = Guid.NewGuid().ToString() },
                new IoTEdgeModule(){ ModuleName = Guid.NewGuid().ToString() }
            };
            var expectedRoutes = Fixture.CreateMany<IoTEdgeRoute>(2).ToList();
            var expectedSysModule = Fixture.CreateMany<EdgeModelSystemModule>(2).ToList();
            var expectedImageUri = Fixture.Create<Uri>();

            var expectedEdgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                ImageUrl = expectedImageUri,
                Description = Guid.NewGuid().ToString(),
                EdgeModules = expectedModules,
                EdgeRoutes = expectedRoutes,
                SystemModules = expectedSysModule
            };

            var expectedCommands = Fixture.CreateMany<EdgeModuleCommand>(5).Select(command =>
            {
                command.EdgeModelId = expectedEdgeDeviceModel.ModelId;
                command.EdgeModuleName = expectedModules[0].ModuleName;
                return command;
            }) .ToList();

            expectedCommands.Add(new EdgeModuleCommand
            {
                EdgeModelId = expectedEdgeDeviceModel.ModelId,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                EdgeModuleName = expectedModules[0].ModuleName,
                Request = new EdgeModuleCommandPayload
                {
                    moduleCommandSchemaType = Shared.Models.v1._0.IoTEdgeModuleCommand.ModuleCommandSchemaType.Object,
                    ComplexSchema = /*lang=json*/ "{ \"Type\": \"Object\", \"Comment\": \"DeviceExport01\", \"Description\": \"DeviceExport01\", \"DisplayName\": \"DeviceExport01\", \"Id\": \"01a440ca-9a67-4334-84a8-0f39995612a4\", \"Field\": [ {\"Name\": \"123\", \"SchemaType\": 1 } ]}"
                }
            });

            var expectedEdgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(expectedEdgeDeviceModel);

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(expectedEdgeDeviceModelEntity);

            _ = this.mockConfigService.Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(expectedModules);

            _ = this.mockConfigService.Setup(x => x.GetModelSystemModule(It.IsAny<string>()))
                .ReturnsAsync(expectedSysModule);

            _ = this.mockConfigService.Setup(x => x.GetConfigRouteList(It.IsAny<string>()))
                .ReturnsAsync(expectedRoutes);

            _ = this.mockEdgeModuleCommandsService.Setup(x => x.GetAllEdgeModuleCommands(It.Is<string>(c => c.Equals(expectedEdgeDeviceModel.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedCommands);

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
            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), m => m.Labels))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.edgeDeviceModelService.GetEdgeModel("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public async Task CreateEdgeModelShouldCreateEdgeModel()
        {
            // Arrange
            var edgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDeviceModel>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeModuleCommandsService
                .Setup(x => x.SaveEdgeModuleCommandAsync(It.Is<string>(c => c.Equals(edgeDeviceModel.ModelId, StringComparison.Ordinal)), It.IsAny<List<IoTEdgeModule>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(It.IsAny<string>()))
                .ReturnsAsync(expectedImageUri.ToString());

            // Act
            await this.edgeDeviceModelService.CreateEdgeModel(edgeDeviceModel);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public void CreateEdgeModelShouldThrowResourceAlreadyExistsExceptionIfModelAlreadyExists()
        {
            // Arrange
            var edgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

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
            var edgeDeviceModel = new IoTEdgeModel() { ModelId = Guid.NewGuid().ToString() };

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
            var edgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Labels = new List<LabelDto>() { new LabelDto { Name = "test", Color = "bleu" } }
            };
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeModuleCommandsService
                .Setup(x => x.SaveEdgeModuleCommandAsync(It.Is<string>(c => c.Equals(edgeDeviceModel.ModelId, StringComparison.Ordinal)), It.IsAny<List<IoTEdgeModule>>()))
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
            var edgeDeviceModel = new IoTEdgeModel() { ModelId = Guid.NewGuid().ToString() };

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), m => m.Labels))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.edgeDeviceModelService.UpdateEdgeModel(edgeDeviceModel);

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public void UpdateEdgeModelShouldThrowInternalServerErrorExceptionIfDbUpdateExceptionOccurs()
        {
            // Arrange
            var edgeDeviceModel = new IoTEdgeModel() { ModelId = Guid.NewGuid().ToString() };
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
            var edgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Labels = new List<LabelDto>() { new LabelDto { Name = "test", Color = "bleu" } }
            };
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetByIdAsync(edgeDeviceModelEntity.Id, d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeModuleCommand>(5).Select(command =>
            {
                command.EdgeModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();

            _ = this.mockEdgeModuleCommandsService
                .Setup(x => x.GetAllEdgeModuleCommands(It.Is<string>(c => c.Equals(edgeDeviceModel.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedCommands);

            _ = this.mockEdgeModuleCommandsService
                .Setup(x => x.DeleteEdgeModuleCommandAsync(It.IsAny<string>()));

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
        public async Task DeleteEdgeModel_ModelDoesntExist_NothingIsDone()
        {
            // Arrange
            var edgeModelId = Fixture.Create<string>();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetByIdAsync(edgeModelId, d => d.Labels))
                .ReturnsAsync(value: null);

            // Act
            await this.edgeDeviceModelService.DeleteEdgeModel(edgeModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void DeleteEdgeModelShouldThrowInternalServerErrorExceptionIfDbUpdateExceptionOccurs()
        {
            // Arrange
            var edgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Labels = new List<LabelDto>() { new LabelDto { Name = "test", Color = "bleu" } }
            };

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Throws(new DbUpdateException());

            var expectedCommands = Fixture.CreateMany<EdgeModuleCommand>(5).Select(command =>
            {
                command.EdgeModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();

            _ = this.mockEdgeModuleCommandsService
                .Setup(x => x.GetAllEdgeModuleCommands(It.Is<string>(c => c.Equals(edgeDeviceModel.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedCommands);

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
    }
}
