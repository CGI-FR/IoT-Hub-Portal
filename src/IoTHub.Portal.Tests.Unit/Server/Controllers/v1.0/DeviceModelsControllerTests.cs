// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    [TestFixture]
    public class DeviceModelsControllerTests : BackendUnitTest
    {
        private Mock<IDeviceModelService<DeviceModelDto, DeviceModelDto>> mockDeviceModelService;

        private DeviceModelsController deviceModelsController;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelService = MockRepository.Create<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelsController = new DeviceModelsController(Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>());
        }

        [Test]
        public async Task GetListShouldReturnAList()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModelDto>(3).ToList();

            var filter = new DeviceModelFilter();

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModels(filter))
                .ReturnsAsync(new PaginatedResult<DeviceModelDto> { Data = expectedDeviceModels });

            // Act
            var response = await this.deviceModelsController.GetItems(filter);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedDeviceModels);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItemShouldReturnAValue()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModel(expectedDeviceModel.ModelId))
                .ReturnsAsync(expectedDeviceModel);

            // Act
            var response = await this.deviceModelsController.GetItem(expectedDeviceModel.ModelId);

            // Assert
            _ = response.Value.Should().BeEquivalentTo(expectedDeviceModel);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvatarShouldReturnTheComputedModelAvatarUri()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();
            var expectedAvatar = Fixture.Create<string>();

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModelAvatar(deviceModel.ModelId))
                .ReturnsAsync(expectedAvatar);

            // Act
            var response = await this.deviceModelsController.GetAvatar(deviceModel.ModelId);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedAvatar);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeAvatarShouldChangeModelImageStream()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();
            var expectedAvatar = DeviceModelImageOptions.DefaultImage;

            _ = this.mockDeviceModelService.Setup(service => service.UpdateDeviceModelAvatar(deviceModel.ModelId, It.IsAny<string>()))
                .ReturnsAsync(expectedAvatar);

            // Act
            var response = await this.deviceModelsController.ChangeAvatar(deviceModel.ModelId, DeviceModelImageOptions.DefaultImage);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedAvatar);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAvatarShouldRemoveModelImage()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelService.Setup(service => service.DeleteDeviceModelAvatar(deviceModel.ModelId))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.deviceModelsController.DeleteAvatar(deviceModel.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateANewEntity()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelService.Setup(service => service.CreateDeviceModel(deviceModel))
                .ReturnsAsync(deviceModel);

            // Act
            _ = await this.deviceModelsController.Post(deviceModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PutShouldUpdateTheDeviceModel()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelService.Setup(service => service.UpdateDeviceModel(deviceModel))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.deviceModelsController.Put(deviceModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShouldRemoveTheEntityCommandsAndAvatar()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelService.Setup(service => service.DeleteDeviceModel(deviceModel.ModelId))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.deviceModelsController.Delete(deviceModel.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
