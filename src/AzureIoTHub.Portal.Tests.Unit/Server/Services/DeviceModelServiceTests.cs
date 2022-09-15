// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Exceptions;
    using Portal.Domain.Repositories;
    using Portal.Server.Managers;
    using UnitTests.Bases;

    [TestFixture]
    public class DeviceModelServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceModelCommandRepository> mockDeviceModelCommandRepository;
        private Mock<IDeviceProvisioningServiceManager> mockDeviceProvisioningServiceManager;
        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>> mockDeviceModelMapper;

        private IDeviceModelService<DeviceModelDto, DeviceModelDto> deviceModelService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();

            this.mockDeviceModelCommandRepository = MockRepository.Create<IDeviceModelCommandRepository>();
            this.mockDeviceProvisioningServiceManager = MockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockConfigService = MockRepository.Create<IConfigService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceService = MockRepository.Create<IDeviceService>();
            this.mockDeviceModelMapper = MockRepository.Create<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceProvisioningServiceManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelMapper.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelService<DeviceModelDto, DeviceModelDto>, DeviceModelService<DeviceModelDto, DeviceModelDto>>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelService = Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();
        }

        [Test]
        public async Task GetDeviceModelsShouldReturnExpectedDeviceModels()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModel>(3).ToList();
            var expectedDeviceModelsDto = expectedDeviceModels.Select(model => Mapper.Map<DeviceModelDto>(model)).ToList();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetAll())
                .Returns(expectedDeviceModels);

            // Act
            var result = await this.deviceModelService.GetDeviceModels();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModelsDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelShouldReturnExpectedDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            var expectedDeviceModelDto = Mapper.Map<DeviceModelDto>(expectedDeviceModel);

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            // Act
            var result = await this.deviceModelService.GetDeviceModel(expectedDeviceModel.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModelDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelShouldThrowResourceNotFoundExceptionWhenDeviceModelNotExist()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelId))
                .ReturnsAsync((DeviceModel)null);

            // Act
            var act = () => this.deviceModelService.GetDeviceModel(deviceModelId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModelShouldCreateDeviceModel()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelMapper
                .Setup(mapper => mapper.BuildDeviceModelDesiredProperties(It.IsAny<DeviceModelDto>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockDeviceProvisioningServiceManager.Setup(manager =>
                    manager.CreateEnrollmentGroupFromModelAsync(deviceModelDto.ModelId, deviceModelDto.Name,
                        It.IsAny<TwinCollection>()))
                .ReturnsAsync((EnrollmentGroup)null);

            _ = this.mockConfigService.Setup(service =>
                    service.RollOutDeviceModelConfiguration(deviceModelDto.ModelId,
                        It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.CreateDeviceModel(deviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModelShouldThrowInternalServerErrorExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceModelService.CreateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldUpdateDeviceModel()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId))
                .ReturnsAsync(new DeviceModel());

            this.mockDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<DeviceModel>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelMapper
                .Setup(mapper => mapper.BuildDeviceModelDesiredProperties(It.IsAny<DeviceModelDto>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockDeviceProvisioningServiceManager.Setup(manager =>
                    manager.CreateEnrollmentGroupFromModelAsync(deviceModelDto.ModelId, deviceModelDto.Name,
                        It.IsAny<TwinCollection>()))
                .ReturnsAsync((EnrollmentGroup)null);

            _ = this.mockConfigService.Setup(service =>
                    service.RollOutDeviceModelConfiguration(deviceModelDto.ModelId,
                        It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldThrowInternalServerErrorExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId))
                .ReturnsAsync(new DeviceModel());

            this.mockDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<DeviceModel>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldThrowResourceNotFoundExceptionWhenDeviceModelDOntExist()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId))
                .ReturnsAsync((DeviceModel)null);

            // Act
            var act = () => this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }
    }
}
