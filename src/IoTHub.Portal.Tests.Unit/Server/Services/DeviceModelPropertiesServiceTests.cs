// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class DeviceModelPropertiesServiceTests : BackendUnitTest
    {
        private Mock<IDeviceModelPropertiesRepository> mockDeviceModelPropertiesRepository;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IDeviceModelPropertiesService deviceModelPropertiesService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelPropertiesRepository = MockRepository.Create<IDeviceModelPropertiesRepository>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelPropertiesRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelPropertiesService, DeviceModelPropertiesService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelPropertiesService = Services.GetRequiredService<IDeviceModelPropertiesService>();
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.GetModelProperties(entity.Id))
                    .ReturnsAsync(new[]
                        {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = entity.Id
                            }
                        });

            // Act
            var result = await this.deviceModelPropertiesService.GetModelProperties(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());

            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceModelNotExistsGetPropertiesShouldReturnHttp404()
        {
            // Arrange
            SetupNotFoundEntity();

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await this.deviceModelPropertiesService.GetModelProperties(Guid.NewGuid().ToString()));
        }

        [Test]
        public void SavePropertiesForModelShouldThrowResourceNotFoundExceptionWhenDeviceModelNotExists()
        {
            // Arrange
            SetupNotFoundEntity();

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await this.deviceModelPropertiesService.SavePropertiesForModel(Guid.NewGuid().ToString(), new List<DeviceModelProperty>()));
        }

        [Test]
        public async Task SetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var entity = SetupMockEntity();

            var properties = new[]
            {
                new DeviceModelProperty
                {
                    DisplayName =Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.Id, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelPropertiesService.SavePropertiesForModel(entity.Id, properties);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenExceptionOccuresSavePropertiesForModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.Id, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Throws<CannotInsertNullException>();

            // Act
            _ = Assert.ThrowsAsync<CannotInsertNullException>(async () => await this.deviceModelPropertiesService.SavePropertiesForModel(entity.Id, Array.Empty<DeviceModelProperty>()));
        }

        [Test]
        public void GetAllPropertiesNamesShouldReturnList()
        {
            var properties = Fixture.CreateMany<DeviceModelProperty>(3).ToList();
            var expectedPropertyNames = properties.Select(tag => tag.Name).ToList();

            _ = this.mockDeviceModelPropertiesRepository.Setup(repository => repository.GetAll())
                .Returns(properties);

            // Act
            var result = this.deviceModelPropertiesService.GetAllPropertiesNames();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedPropertyNames);

            MockRepository.VerifyAll();
        }

        private DeviceModel SetupMockEntity()
        {
            var modelId = Guid.NewGuid().ToString();
            var deviceModelEntity = new DeviceModel
            {
                Id = modelId
            };

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(modelId))
                .ReturnsAsync(deviceModelEntity);

            return deviceModelEntity;
        }

        private void SetupNotFoundEntity()
        {
            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((DeviceModel)null);
        }
    }
}
