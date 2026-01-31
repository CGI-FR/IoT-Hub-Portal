// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using DeviceEntity = Portal.Domain.Entities.Device;
    using IncompatibleDeviceModelException = Portal.Domain.Exceptions.IncompatibleDeviceModelException;
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class LayerServiceTests : BackendUnitTest
    {
        private Mock<ILayerRepository> mockLayerRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IPlanningRepository> mockPlanningRepository;
        private Mock<IDeviceRepository> mockDeviceRepository;

        private ILayerService layerService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockLayerRepository = MockRepository.Create<ILayerRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockPlanningRepository = MockRepository.Create<IPlanningRepository>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();

            _ = ServiceCollection.AddSingleton(this.mockLayerRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockPlanningRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton<ILayerService, LayerService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.layerService = Services.GetRequiredService<ILayerService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateNewEntity()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.InsertAsync(It.IsAny<Layer>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await this.layerService.CreateLayer(expectedLayerDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLayerDto.Name, result.Name);
            Assert.AreEqual(expectedLayerDto.Father, result.Father);
            Assert.AreEqual(expectedLayerDto.Planning, result.Planning);
            Assert.AreEqual(expectedLayerDto.hasSub, result.hasSub);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateShouldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.UpdateLayer(expectedLayerDto);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteExistigLayerSuccessfully()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
               .ReturnsAsync(expectedLayer);

            this.mockLayerRepository.Setup(repository => repository.Delete(expectedLayerDto.Id))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            await this.layerService.DeleteLayer(expectedLayerDto.Id);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShuldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.DeleteLayer(expectedLayerDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldReturnValue()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
               .ReturnsAsync(expectedLayer);

            var result = await this.layerService.GetLayer(expectedLayerDto.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLayerDto.Name, result.Name);
            Assert.AreEqual(expectedLayerDto.Father, result.Father);
            Assert.AreEqual(expectedLayerDto.Planning, result.Planning);
            Assert.AreEqual(expectedLayerDto.hasSub, result.hasSub);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayerShouldThrowResourceNotFoundException()
        {
            var expectedLayer = Fixture.Create<Layer>();
            var expectedLayerDto = Mapper.Map<LayerDto>(expectedLayer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(expectedLayerDto.Id))
                .ReturnsAsync((Layer)null);

            var act = () => this.layerService.GetLayer(expectedLayerDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The layer with id {expectedLayer.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetLayersShouldReturnLayerList()
        {
            var expectedTotalLayersCount = 50;
            var expectedDevices = Fixture.CreateMany<Layer>(expectedTotalLayersCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockLayerRepository.Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDevices);

            var result = await this.layerService.GetLayers();

            _ = result.Count().Should().Be(expectedTotalLayersCount);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateLayer_WhenPlanningHasDeviceModelIdAndLayerHasIncompatibleDevices_ShouldThrowIncompatibleDeviceModelException()
        {
            // Arrange
            var deviceModelId1 = Fixture.Create<string>();
            var deviceModelId2 = Fixture.Create<string>();

            var planning = Fixture.Build<Planning>()
                .With(p => p.DeviceModelId, deviceModelId1)
                .Create();

            var layer = Fixture.Build<Layer>()
                .With(l => l.Planning, planning.Id)
                .Create();

            var layerDto = Mapper.Map<LayerDto>(layer);

            var device = Fixture.Build<DeviceEntity>()
                .With(d => d.LayerId, layer.Id)
                .With(d => d.DeviceModelId, deviceModelId2)
                .Create();

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(layer.Id))
                .ReturnsAsync(layer);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(planning.Id))
                .ReturnsAsync(planning);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<DeviceEntity, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceEntity, object>>[]>()))
                .ReturnsAsync(new List<DeviceEntity> { device });

            _ = this.mockLayerRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Layer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Layer, object>>[]>()))
                .ReturnsAsync(new List<Layer>());

            // Act
            var act = () => this.layerService.UpdateLayer(layerDto);

            // Assert
            _ = await act.Should().ThrowAsync<IncompatibleDeviceModelException>()
                .WithMessage($"Cannot link layer '{layer.Name}' to planning. The layer contains 1 device(s) with a different device model than required by the planning.");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateLayer_WhenPlanningHasDeviceModelIdAndLayerHasNoDevices_ShouldSucceed()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var planning = Fixture.Build<Planning>()
                .With(p => p.DeviceModelId, deviceModelId)
                .Create();

            var layer = Fixture.Build<Layer>()
                .With(l => l.Planning, planning.Id)
                .Create();

            var layerDto = Mapper.Map<LayerDto>(layer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(layer.Id))
                .ReturnsAsync(layer);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(planning.Id))
                .ReturnsAsync(planning);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<DeviceEntity, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceEntity, object>>[]>()))
                .ReturnsAsync(new List<DeviceEntity>());

            _ = this.mockLayerRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Layer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Layer, object>>[]>()))
                .ReturnsAsync(new List<Layer>());

            this.mockLayerRepository.Setup(repository => repository.Update(It.IsAny<Layer>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.layerService.UpdateLayer(layerDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateLayer_WhenPlanningHasDeviceModelIdAndLayerHasCompatibleDevices_ShouldSucceed()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var planning = Fixture.Build<Planning>()
                .With(p => p.DeviceModelId, deviceModelId)
                .Create();

            var layer = Fixture.Build<Layer>()
                .With(l => l.Planning, planning.Id)
                .Create();

            var layerDto = Mapper.Map<LayerDto>(layer);

            var device = Fixture.Build<DeviceEntity>()
                .With(d => d.LayerId, layer.Id)
                .With(d => d.DeviceModelId, deviceModelId)
                .Create();

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(layer.Id))
                .ReturnsAsync(layer);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(planning.Id))
                .ReturnsAsync(planning);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<DeviceEntity, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceEntity, object>>[]>()))
                .ReturnsAsync(new List<DeviceEntity> { device });

            _ = this.mockLayerRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Layer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Layer, object>>[]>()))
                .ReturnsAsync(new List<Layer>());

            this.mockLayerRepository.Setup(repository => repository.Update(It.IsAny<Layer>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.layerService.UpdateLayer(layerDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateLayer_WhenPlanningHasNoDeviceModelId_ShouldSucceed()
        {
            // Arrange
            var planning = Fixture.Build<Planning>()
                .Without(p => p.DeviceModelId)
                .Create();

            var layer = Fixture.Build<Layer>()
                .With(l => l.Planning, planning.Id)
                .Create();

            var layerDto = Mapper.Map<LayerDto>(layer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(layer.Id))
                .ReturnsAsync(layer);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(planning.Id))
                .ReturnsAsync(planning);

            this.mockLayerRepository.Setup(repository => repository.Update(It.IsAny<Layer>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.layerService.UpdateLayer(layerDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateLayer_WhenPlanningDoesNotExist_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var planningId = Fixture.Create<string>();

            var layer = Fixture.Build<Layer>()
                .With(l => l.Planning, planningId)
                .Create();

            var layerDto = Mapper.Map<LayerDto>(layer);

            _ = this.mockLayerRepository.Setup(repository => repository.GetByIdAsync(layer.Id))
                .ReturnsAsync(layer);

            _ = this.mockPlanningRepository.Setup(repository => repository.GetByIdAsync(planningId))
                .ReturnsAsync((Planning)null);

            // Act
            var act = () => this.layerService.UpdateLayer(layerDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>()
                .WithMessage($"The planning with id {planningId} doesn't exist");

            MockRepository.VerifyAll();
        }
    }
}
