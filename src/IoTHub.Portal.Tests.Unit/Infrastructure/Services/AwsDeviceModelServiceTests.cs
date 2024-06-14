// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using IoTHub.Portal.Infrastructure.Services;
    using Moq;
    using AutoFixture;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using FluentAssertions;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Domain.Exceptions;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using IoTHub.Portal.Domain.Repositories;

    [TestFixture]
    public class AwsDeviceModelServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IExternalDeviceService> mockExternalDeviceService;

        private IDeviceModelService<DeviceModelDto, DeviceModelDto> deviceModelService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelService<DeviceModelDto, DeviceModelDto>, AwsDeviceModelService<DeviceModelDto, DeviceModelDto>>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelService = Services.GetRequiredService<IDeviceModelService<DeviceModelDto, DeviceModelDto>>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetDeviceModels_WithoutFilter_DeviceModelsReturned()
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

            _ = this.mockDeviceModelRepository.Setup(u => u.GetPaginatedListAsync(filter.PageNumber, filter.PageSize, filter.OrderBy, It.IsAny<Expression<Func<DeviceModel, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
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
        public async Task GetDeviceModel_ExistingDeviceModel_DeviceModelReturned()
        {
            // Arrange
            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            var expectedDeviceModelDto = Mapper.Map<DeviceModelDto>(expectedDeviceModel);

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(expectedDeviceModel.Id, d => d.Labels))
                .ReturnsAsync(expectedDeviceModel);

            // Act
            var result = await this.deviceModelService.GetDeviceModel(expectedDeviceModel.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModelDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModel_ValidDeviceModel_DeviceModelCreated()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();
            var externalDeviceModelDto = Mapper.Map<ExternalDeviceModelDto>(deviceModelDto);
            externalDeviceModelDto.Id = Fixture.Create<string>();

            var expectedAvatarUrl = Fixture.Create<string>();

            _ = this.mockExternalDeviceService.Setup(e => e.CreateDeviceModel(It.Is<ExternalDeviceModelDto>(d => externalDeviceModelDto.Name.Equals(d.Name, StringComparison.Ordinal))))
                .ReturnsAsync(externalDeviceModelDto);

            _ = this.mockDeviceModelRepository.Setup(u => u.InsertAsync(It.IsAny<DeviceModel>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(u => u.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(externalDeviceModelDto.Id))
                .ReturnsAsync(expectedAvatarUrl);

            // Act
            var result = await this.deviceModelService.CreateDeviceModel(deviceModelDto);

            // Assert
            _ = result.ModelId.Should().Be(externalDeviceModelDto.Id);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModel_ExistingDeviceModel_DeviceModelUpdated()
        {
            // Arrange
            var existingDeviceModel = Fixture.Create<DeviceModel>();

            var deviceModelDto = Mapper.Map<DeviceModelDto>(existingDeviceModel);
            deviceModelDto.Labels = Fixture.CreateMany<LabelDto>(10).ToList();

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(existingDeviceModel);

            this.mockDeviceModelRepository.Setup(u => u.Update(It.Is<DeviceModel>(d => deviceModelDto.ModelId.Equals(d.Id, StringComparison.Ordinal))))
                .Verifiable();

            this.mockLabelRepository.Setup(u => u.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceModel_NonExistingDeviceModel_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceModelDto = Fixture.Create<DeviceModelDto>();

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(deviceModelDto.ModelId, d => d.Labels))
                .ReturnsAsync(default(DeviceModel));
            // Act
            var act = () => this.deviceModelService.UpdateDeviceModel(deviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_ExistingDeviceModel_DeviceModelDeleted()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            var externalDeviceModelDto = new ExternalDeviceModelDto
            {
                Name = deviceModel.Name,
            };

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(deviceModel.Id, d => d.Labels))
                .ReturnsAsync(deviceModel);

            _ = this.mockExternalDeviceService.Setup(e => e.DeleteDeviceModel(It.Is<ExternalDeviceModelDto>(d => externalDeviceModelDto.Name.Equals(d.Name, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            this.mockLabelRepository.Setup(u => u.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.DeleteDeviceModelImageAsync(deviceModel.Id))
                .Returns(Task.CompletedTask);

            this.mockDeviceModelRepository.Setup(u => u.Delete(deviceModel.Id))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceModelService.DeleteDeviceModel(deviceModel.Id);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_NonExistingDeviceModel_DeviceModelIsNotDeleted()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(deviceModel.Id, d => d.Labels))
                .ReturnsAsync(default(DeviceModel));

            // Act
            await this.deviceModelService.DeleteDeviceModel(deviceModel.Id);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceModelAvatar_ExistingDeviceModelAvatar_DeviceModelAvatarReturned()
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
        public async Task UpdateDeviceModelAvatar_ExistingDeviceModelAvatar_DeviceModelAvatarUpdated()
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
        public async Task DeleteDeviceModelAvatar_ExistingDeviceModelAvatar_DeviceModelAvatarDeleted()
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
