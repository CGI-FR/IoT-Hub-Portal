// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using Shared.Constants;

    public class EdgeModelServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDeviceModelRepository> mockEdgeDeviceModelRepository;
        private Mock<IEdgeDeviceModelCommandRepository> mockEdgeDeviceModelCommandRepository;
        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<ConfigHandler> mockConfigHandler;

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
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton<IEdgeModelService, EdgeModelService>();
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.edgeDeviceModelService = Services.GetRequiredService<IEdgeModelService>();
            Mapper = Services.GetRequiredService<IMapper>();

        }

        [Test]
        public async Task GetEdgeModelsShouldReturnAList()
        {
            // Arrange
            var expectedEdgeDeviceModels = Fixture.CreateMany<EdgeDeviceModel>(3).ToList();
            var expectedIoTEdgeDeviceModelListItems = expectedEdgeDeviceModels.Select(model => Mapper.Map<IoTEdgeModelListItem>(model)).ToList();
            var expectedImage = DeviceModelImageOptions.DefaultImage;

            foreach (var item in expectedIoTEdgeDeviceModelListItems)
            {
                item.Image = expectedImage;
            }

            var edgeModelFilter = new EdgeModelFilter
            {
                Keyword = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<EdgeDeviceModel, bool>>>(), default, d => d.Labels))
                .ReturnsAsync(expectedEdgeDeviceModels);

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.IsAny<string>()).Result)
                .Returns(expectedImage);

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
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("Azure");

            var expectedModules = Fixture.CreateMany<IoTEdgeModule>(2).ToList();
            var expectedRoutes = Fixture.CreateMany<IoTEdgeRoute>(2).ToList();
            var expectedSysModule = Fixture.CreateMany<EdgeModelSystemModule>(2).ToList();
            const string expectedImage = DeviceModelImageOptions.DefaultImage;

            var expectedEdgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Image = expectedImage,
                Description = Guid.NewGuid().ToString(),
                EdgeModules = expectedModules,
                EdgeRoutes = expectedRoutes,
                SystemModules = expectedSysModule
            };

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = expectedEdgeDeviceModel.ModelId;
                command.ModuleName = expectedModules[0].ModuleName;
                return command;
            }) .ToList();

            expectedCommands.Add(new EdgeDeviceModelCommand
            {
                EdgeDeviceModelId = expectedEdgeDeviceModel.ModelId,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                ModuleName = Guid.NewGuid().ToString()
            });

            var expectedEdgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(expectedEdgeDeviceModel);

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.IsAny<string>()).Result)
                .Returns(expectedImage);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(expectedEdgeDeviceModelEntity);

            _ = this.mockConfigService.Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(expectedModules);

            _ = this.mockConfigService.Setup(x => x.GetModelSystemModule(It.IsAny<string>()))
                .ReturnsAsync(expectedSysModule);

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
        public async Task GetEdgeModelForAwsShouldReturnValueAsync()
        {
            // Arrange
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("AWS");

            var expectedModules = Fixture.CreateMany<IoTEdgeModule>(2).ToList();

            var expectedImage = DeviceModelImageOptions.DefaultImage;

            var expectedEdgeDeviceModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Image = expectedImage,
                Description = Guid.NewGuid().ToString(),
                EdgeModules = expectedModules
            };

            var expectedEdgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(expectedEdgeDeviceModel);

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.IsAny<string>()).Result)
                .Returns(expectedImage);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(expectedEdgeDeviceModelEntity);

            _ = this.mockConfigService.Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(expectedModules);

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
        public async Task CreateEdgeModelForAzureShouldCreateEdgeModel()
        {
            // Arrange

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var expectedImage = Fixture.Create<Uri>();

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDeviceModel>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("Azure");

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();

            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDeviceModelCommand, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCommands);

            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.InsertAsync(It.IsAny<EdgeDeviceModelCommand>()))
                 .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(It.IsAny<string>()))
                .ReturnsAsync(expectedImage.ToString());

            // Act
            await this.edgeDeviceModelService.CreateEdgeModel(edgeDeviceModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeModelForAwsShouldCreateEdgeModel()
        {
            // Arrange

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var expectedImage = Fixture.Create<Uri>();

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);
            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDeviceModel>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("AWS");


            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(It.IsAny<string>()))
                .ReturnsAsync(expectedImage.ToString());

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
                .ReturnsAsync(value: null);
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
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("Azure");

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }).ToList();

            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDeviceModelCommand, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCommands);

            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.Delete(It.IsAny<string>()));
            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.InsertAsync(It.IsAny<EdgeDeviceModelCommand>()))
                .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            // Act
            await this.edgeDeviceModelService.UpdateEdgeModel(edgeDeviceModel);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public async Task UpdateEdgeModelForAwsShouldUpdateEdgeModel()
        {
            // Arrange
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("AWS");

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
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
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("Azure");

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetByIdAsync(edgeDeviceModelEntity.Id, d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var expectedCommands = Fixture.CreateMany<EdgeDeviceModelCommand>(5).Select(command =>
            {
                command.EdgeDeviceModelId = edgeDeviceModel.ModelId;
                return command;
            }) .ToList();

            _ = this.mockEdgeDeviceModelCommandRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDeviceModelCommand, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCommands);

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
        public async Task DeleteEdgeModelForAwsShouldDeleteEdgeModel()
        {
            // Arrange
            _ = this.mockConfigHandler.Setup(handler => handler.CloudProvider).Returns("AWS");

            var edgeDeviceModel = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edgeDeviceModel);

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.GetByIdAsync(edgeDeviceModelEntity.Id, d => d.Labels))
                .ReturnsAsync(edgeDeviceModelEntity);

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockEdgeDeviceModelRepository.Setup(repository => repository.Delete(It.IsAny<string>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

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
            var expectedImage = DeviceModelImageOptions.DefaultImage;
            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.IsAny<string>()).Result)
                .Returns(expectedImage);

            // Act
            var result = await this.edgeDeviceModelService.GetEdgeModelAvatar(Guid.NewGuid().ToString());

            // Assert
            _ = result.Should().Be(expectedImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeDeviceModelAvatarShouldUpdateEdgeDeviceModelAvatar()
        {
            // Arrange
            var image= DeviceModelImageOptions.DefaultImage;

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.ChangeDeviceModelImageAsync(It.IsAny<string>(), DeviceModelImageOptions.DefaultImage))
                .ReturnsAsync(image);

            // Act
            var result = await this.edgeDeviceModelService.UpdateEdgeModelAvatar(Guid.NewGuid().ToString(),
                DeviceModelImageOptions.DefaultImage);

            // Assert
            _ = result.Should().Be(image);
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

        [Test]
        public async Task GetPublicEdgeModulesGetPublicEdgeModulesEdgeModulesReturned()
        {
            // Arrange
            var edgeModules = Fixture.CreateMany<IoTEdgeModule>(10).ToList();

            _ = this.mockConfigService
                .Setup(s => s.GetPublicEdgeModules())
                .ReturnsAsync(edgeModules);

            // Act
            var result = await this.edgeDeviceModelService.GetPublicEdgeModules();

            // Assert
            _ = result.Should().BeEquivalentTo(edgeModules);
            MockRepository.VerifyAll();
        }
    }
}
