// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;

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

            _ = this.mockDeviceModelService.Setup(service => service.GetDeviceModels())
                .ReturnsAsync(expectedDeviceModels);

            // Act
            var response = await this.deviceModelsController.GetItems();

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
            var expectedAvatar = Fixture.Create<string>();

            _ = this.mockDeviceModelService.Setup(service => service.UpdateDeviceModelAvatar(deviceModel.ModelId, It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedAvatar);

            // Act
            var response = await this.deviceModelsController.ChangeAvatar(deviceModel.ModelId, MockRepository.Create<IFormFile>().Object);

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
                .Returns(Task.CompletedTask);

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
