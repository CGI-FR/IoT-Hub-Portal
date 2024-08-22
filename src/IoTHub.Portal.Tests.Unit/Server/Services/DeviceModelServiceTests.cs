// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Providers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Infrastructure.Mappers;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Exceptions;
    using Portal.Domain.Repositories;
    using Shared;
    using Shared.Models.v1._0.Filters;
    using UnitTests.Bases;

    [TestFixture]
    public class DeviceModelServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceModelCommandRepository> mockDeviceModelCommandRepository;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IDeviceRegistryProvider> mockRegistryProvider;
        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>> mockDeviceModelMapper;

        private IDeviceModelService<DeviceModelDto, DeviceModelDto> deviceModelService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockDeviceModelCommandRepository = MockRepository.Create<IDeviceModelCommandRepository>();
            this.mockRegistryProvider = MockRepository.Create<IDeviceRegistryProvider>();
            this.mockConfigService = MockRepository.Create<IConfigService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceModelMapper = MockRepository.Create<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelCommandRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockRegistryProvider.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelMapper.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelService<DeviceModelDto, DeviceModelDto>, DeviceModelService<DeviceModelDto, DeviceModelDto>>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelService = Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetDeviceModelsShouldReturnExpectedDeviceModels()
        {
            // Arrange
            var expectedDeviceModels = Fixture.CreateMany<DeviceModel>(3).ToList();
            var expectedImageUri = Fixture.Create<Uri>();
            var expectedDeviceModelsDto = expectedDeviceModels.Select(model =>
            {
                var deviceModelDto = Mapper.Map<DeviceModelDto>(model);
                deviceModelDto.ImageUrl = expectedImageUri;
                return deviceModelDto;
            }).ToList();

            var filter = new DeviceModelFilter
            {
                SearchText = Fixture.Create<string>(),
                PageNumber = 1,
                PageSize = 10,
                OrderBy = new string[]
                {
                    null
                }
            };

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetPaginatedListAsync(filter.PageNumber, filter.PageSize, filter.OrderBy, It.IsAny<Expression<Func<DeviceModel, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(new PaginatedResult<DeviceModel>
                {
                    Data = expectedDeviceModels,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.PageNumber,
                    TotalCount = 10
                });

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            // Act
            var result = await this.deviceModelService.GetDeviceModels(filter);

            // Assert
            _ = result.Data.Should().BeEquivalentTo(expectedDeviceModelsDto);
            _ = result.CurrentPage.Should().Be(filter.PageNumber);
            _ = result.PageSize.Should().Be(filter.PageSize);
            _ = result.TotalCount.Should().Be(10);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelShouldReturnExpectedDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            var expectedDeviceModelDto = Mapper.Map<DeviceModelDto>(expectedDeviceModel);

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceModel.Id, d => d.Labels))
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

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelId, d => d.Labels))
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
            var expectedAvatarUrl = Fixture.Create<string>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelMapper
                .Setup(mapper => mapper.BuildDeviceModelDesiredProperties(It.IsAny<DeviceModelDto>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockRegistryProvider.Setup(manager =>
                    manager.CreateEnrollmentGroupFromModelAsync(deviceModelDto.ModelId, deviceModelDto.Name,
                        It.IsAny<TwinCollection>()))
                .ReturnsAsync((EnrollmentGroup)null);

            _ = this.mockConfigService.Setup(service =>
                    service.RollOutDeviceModelConfiguration(deviceModelDto.ModelId,
                        It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(deviceModelDto.ModelId))
                .ReturnsAsync(expectedAvatarUrl);

            // Act
            _ = await this.deviceModelService.CreateDeviceModel(deviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModelShouldThrowCannotInsertNullExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new CannotInsertNullException());

            // Act
            var act = () => this.deviceModelService.CreateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<CannotInsertNullException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldUpdateDeviceModel()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(new DeviceModel
                {
                    Labels = Fixture.CreateMany<Label>(2).ToList()
                });

            this.mockDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<DeviceModel>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelMapper
                .Setup(mapper => mapper.BuildDeviceModelDesiredProperties(It.IsAny<DeviceModelDto>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockRegistryProvider.Setup(manager =>
                    manager.CreateEnrollmentGroupFromModelAsync(deviceModelDto.ModelId, deviceModelDto.Name,
                        It.IsAny<TwinCollection>()))
                .ReturnsAsync((EnrollmentGroup)null);

            _ = this.mockConfigService.Setup(service =>
                    service.RollOutDeviceModelConfiguration(deviceModelDto.ModelId,
                        It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            // Act
            await this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldThrowMaxLengthExceededExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(new DeviceModel
                {
                    Labels = new List<Label>()
                });

            this.mockDeviceModelRepository.Setup(repository => repository.Update(It.IsAny<DeviceModel>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new MaxLengthExceededException());

            // Act
            var act = () => this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<MaxLengthExceededException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelShouldThrowResourceNotFoundExceptionWhenDeviceModelDOntExist()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync((DeviceModel)null);

            // Act
            var act = () => this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelShouldDeleteDeviceModel()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            var commands = Fixture.CreateMany<DeviceModelCommand>(5).Select(command =>
            {
                command.DeviceModelId = deviceModelDto.ModelId;
                return command;
            }) .ToList();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(new DeviceModel
                {
                    Labels = Fixture.CreateMany<Label>(2).ToList()
                });

            _ = this.mockDeviceService.Setup(service => service.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = Array.Empty<Twin>()
                });

            _ = this.mockRegistryProvider.Setup(repository => repository.DeleteEnrollmentGroupByDeviceModelIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(repository => repository.DeleteDeviceModelConfigurationByConfigurationNamePrefix(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelCommandRepository.Setup(repository => repository.GetAll())
                .Returns(commands);

            this.mockDeviceModelCommandRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockDeviceModelImageManager
                .Setup(manager => manager.DeleteDeviceModelImageAsync(deviceModelDto.ModelId))
                .Returns(Task.CompletedTask);

            this.mockDeviceModelRepository.Setup(repository => repository.Delete(deviceModelDto.ModelId))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.DeleteDeviceModel(deviceModelDto.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelShouldThrowAnErrorExceptionWhenDDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            var commands = Fixture.CreateMany<DeviceModelCommand>(5).Select(command =>
            {
                command.DeviceModelId = deviceModelDto.ModelId;
                return command;
            }) .ToList();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(new DeviceModel
                {
                    Labels = new List<Label>()
                });

            _ = this.mockDeviceService.Setup(service => service.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = Array.Empty<Twin>()
                });

            _ = this.mockRegistryProvider.Setup(repository => repository.DeleteEnrollmentGroupByDeviceModelIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ = this.mockConfigService.Setup(repository => repository.DeleteDeviceModelConfigurationByConfigurationNamePrefix(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelCommandRepository.Setup(repository => repository.GetAll())
                .Returns(commands);

            this.mockDeviceModelCommandRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockDeviceModelImageManager
                .Setup(manager => manager.DeleteDeviceModelImageAsync(deviceModelDto.ModelId))
                .Returns(Task.CompletedTask);

            this.mockDeviceModelRepository.Setup(repository => repository.Delete(deviceModelDto.ModelId))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceModelService.DeleteDeviceModel(deviceModelDto.ModelId);

            // Assert
            _ = await act.Should().ThrowAsync<DbUpdateException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelShouldThrowInternalServerErrorExceptionWhenDeviceModelIsUsedByDevice()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(new DeviceModel());

            _ = this.mockDeviceService.Setup(service => service.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        new()
                        {
                            Tags = new TwinCollection
                            {
                                ["modelId"] = deviceModelDto.ModelId
                            }
                        }
                    }
                });

            // Act
            var act = () => this.deviceModelService.DeleteDeviceModel(deviceModelDto.ModelId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceAlreadyExistsException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelShouldNotDeleteNonExistingDevice()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(repository => repository.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync((DeviceModel)null);

            // Act
            await this.deviceModelService.DeleteDeviceModel(deviceModelDto.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelAvatarShouldReturnDeviceModelAvatar()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();
            var expectedAvatarUrl = Fixture.Create<Uri>();
            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(deviceModelDto.ModelId))
                .Returns(expectedAvatarUrl);

            // Act
            var result = await this.deviceModelService.GetDeviceModelAvatar(deviceModelDto.ModelId);

            // Assert
            _ = result.Should().Be(expectedAvatarUrl.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModelAvatarShouldUpdateDeviceModelAvatar()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();
            var expectedAvatarUrl = Fixture.Create<string>();

            var mockFormFile = MockRepository.Create<IFormFile>();

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.ChangeDeviceModelImageAsync(deviceModelDto.ModelId, It.IsAny<Stream>()))
                .ReturnsAsync(expectedAvatarUrl);

            _ = mockFormFile.Setup(file => file.OpenReadStream())
                .Returns(Stream.Null);

            // Act
            var result = await this.deviceModelService.UpdateDeviceModelAvatar(deviceModelDto.ModelId, mockFormFile.Object);

            // Assert
            _ = result.Should().Be(expectedAvatarUrl);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModelAvatarShouldDeleteDeviceModelAvatar()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelImageManager
                .Setup(manager => manager.DeleteDeviceModelImageAsync(deviceModelDto.ModelId))
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.DeleteDeviceModelAvatar(deviceModelDto.ModelId);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
