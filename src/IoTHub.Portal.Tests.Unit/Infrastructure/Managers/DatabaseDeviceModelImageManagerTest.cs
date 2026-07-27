// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class DatabaseDeviceModelImageManagerTest : BackendUnitTest
    {
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IDeviceModelImageManager deviceModelImageManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelImageManager, DatabaseDeviceModelImageManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelImageManager = Services.GetRequiredService<IDeviceModelImageManager>();
        }

        [Test]
        public async Task GetDeviceModelImageAsyncShouldReturnStoredImage()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModel.Id, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(deviceModel);

            // Act
            var result = await this.deviceModelImageManager.GetDeviceModelImageAsync(deviceModel.Id);

            // Assert
            _ = result.Should().Be(deviceModel.Image);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelImageAsyncShouldReturnDefaultImageWhenNotFound()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModelId, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync((DeviceModel)null);

            // Act
            var result = await this.deviceModelImageManager.GetDeviceModelImageAsync(deviceModelId);

            // Assert
            _ = result.Should().Be(DeviceModelImageOptions.DefaultImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeDeviceModelImageAsyncShouldUpdateImageAndReturnItsValue()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();
            var expectedImage = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModel.Id, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.Update(deviceModel));

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModel.Id, expectedImage);

            // Assert
            _ = result.Should().Be(expectedImage);
            _ = deviceModel.Image.Should().Be(expectedImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public void ChangeDeviceModelImageAsyncShouldThrowResourceNotFoundExceptionWhenDeviceModelDoesNotExist()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var file = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModelId, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync((DeviceModel)null);

            // Act
            var act = () => this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, file);

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SetDefaultImageToModelShouldUpdateImageAndReturnDefaultImage()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModel.Id, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.Update(deviceModel));

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.Id);

            // Assert
            _ = result.Should().Be(DeviceModelImageOptions.DefaultImage);
            _ = deviceModel.Image.Should().Be(DeviceModelImageOptions.DefaultImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelImageAsyncShouldClearImage()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModel.Id, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.Update(deviceModel));

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModel.Id);

            // Assert
            _ = deviceModel.Image.Should().BeNull();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelImageAsyncShouldDoNothingWhenDeviceModelDoesNotExist()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(deviceModelId, It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync((DeviceModel)null);

            // Act
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task InitializeDefaultImageBlobShouldNotThrow()
        {
            // Act
            await this.deviceModelImageManager.InitializeDefaultImageBlob();

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SyncImagesCacheControlShouldNotThrow()
        {
            // Act
            await this.deviceModelImageManager.SyncImagesCacheControl();

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
