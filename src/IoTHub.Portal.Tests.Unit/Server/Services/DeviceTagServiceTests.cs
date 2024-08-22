// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Services;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using Shared.Models.v1._0;
    using UnitTests.Bases;

    [TestFixture]
    public class DeviceTagServiceTests : BackendUnitTest
    {
        private Mock<IDeviceTagRepository> mockDeviceTagRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IDeviceTagService deviceTagService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceTagRepository = MockRepository.Create<IDeviceTagRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceTagRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IDeviceTagService, DeviceTagService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceTagService = Services.GetRequiredService<IDeviceTagService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task UpdateShouldCreateNewEntity()
        {
            var tag = new DeviceTagDto
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(Array.Empty<DeviceTag>());

            _ = this.mockDeviceTagRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceTag>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceTagService.UpdateTags(new List<DeviceTagDto>(new[] { tag }));

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTagsShouldDeleteExistingTagsAndCreatedNewTags()
        {
            var existingTags = Fixture.CreateMany<DeviceTag>(1).ToList();

            var newTags = Fixture.CreateMany<DeviceTagDto>(1).ToList();

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(existingTags);

            this.mockDeviceTagRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockDeviceTagRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceTag>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceTagService.UpdateTags(newTags);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTagsShouldThrowInternalServerErrorExceptionWhenDDbUpdateExceptionIsThrown()
        {
            var tag = new DeviceTagDto
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(Array.Empty<DeviceTag>());

            _ = this.mockDeviceTagRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceTag>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceTagService.UpdateTags(new List<DeviceTagDto>(new[] { tag }));

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsShouldReturnAList()
        {
            // Arrange
            var tags = Fixture.CreateMany<DeviceTag>(1).ToList();
            var expectedTags = tags.Select(tag => Mapper.Map<DeviceTagDto>(tag)).ToList();

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(tags);

            // Act
            var result = this.deviceTagService.GetAllTags();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTags);

            MockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsNamesShouldReturnAList()
        {
            // Arrange
            var tags = Fixture.CreateMany<DeviceTag>(1).ToList();
            var expectedTagNames = tags.Select(tag => tag.Name).ToList();

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(tags);

            // Act
            var result = this.deviceTagService.GetAllTagsNames();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTagNames);

            MockRepository.VerifyAll();
        }

        [Test]
        public void GetAllSearchableTagsNamesShouldReturnAList()
        {
            // Arrange
            var tags = Fixture.CreateMany<DeviceTag>(1).Select(tag =>
            {
                tag.Searchable = true;
                return tag;
            }).ToList();
            var expectedTagNames = tags.Select(tag => tag.Name).ToList();

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetAll())
                .Returns(tags);

            // Act
            var result = this.deviceTagService.GetAllSearchableTagsNames();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTagNames);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldInsertDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((DeviceTag)null);

            _ = this.mockDeviceTagRepository.Setup(repository => repository.InsertAsync(It.IsAny<DeviceTag>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var act = () => this.deviceTagService.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            _ = await act.Should().NotThrowAsync();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldUpdateExistingDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(Mapper.Map<DeviceTag>(deviceTag));

            this.mockDeviceTagRepository.Setup(repository => repository.Update(It.IsAny<DeviceTag>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var act = () => this.deviceTagService.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            _ = await act.Should().NotThrowAsync();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceTagRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(Mapper.Map<DeviceTag>(deviceTag));

            this.mockDeviceTagRepository.Setup(repository => repository.Update(It.IsAny<DeviceTag>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceTagService.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceTagByNameShouldDeleteDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString()
            };

            this.mockDeviceTagRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var act = () => this.deviceTagService.DeleteDeviceTagByName(deviceTag.Name);

            // Assert
            _ = await act.Should().NotThrowAsync();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceTagByNameShouldThrowInternalServerErrorExceptionWhenAnIssueOccurs()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString()
            };

            this.mockDeviceTagRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceTagService.DeleteDeviceTagByName(deviceTag.Name);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }
    }
}
