// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10.LoRaWAN
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Moq;
    using NUnit.Framework;
    using Portal.Server.Controllers.V10.LoRaWAN;
    using UnitTests.Bases;

    [TestFixture]
    public class LoRaWANDeviceModelsControllerTest : BackendUnitTest
    {
        private Mock<IDeviceModelService<DeviceModelDto, LoRaDeviceModelDto>> mockDeviceModelService;

        private LoRaWANDeviceModelsController deviceModelsController;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelService = MockRepository.Create<IDeviceModelService<DeviceModelDto, LoRaDeviceModelDto>>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelsController = new LoRaWANDeviceModelsController(Services.GetRequiredService<IDeviceModelService<DeviceModelDto, LoRaDeviceModelDto>>());
        }

        [Test]
        public async Task GetListShouldReturnAList()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModelDto>(3).Select(dto =>
            {
                dto.SupportLoRaFeatures = true;
                return dto;
            }) .ToList();

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
            var expectedDeviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

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
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

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
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

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
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = Fixture.Create<string>()
            };

            _ = this.mockDeviceModelService.Setup(service => service.DeleteDeviceModel(deviceModel.ModelId))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.deviceModelsController.Delete(deviceModel.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
