// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using FluentAssertions;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MenuEntryServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IMenuEntryRepository> mockMenuEntryRepository;
        private IMenuEntryService menuEntryService;

        [SetUp]
        public void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockMenuEntryRepository = MockRepository.Create<IMenuEntryRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockMenuEntryRepository.Object);
            _ = ServiceCollection.AddSingleton<IMenuEntryService, MenuEntryService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.menuEntryService = Services.GetRequiredService<IMenuEntryService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateMenuEntry_ValidDto_ShouldCreateSuccessfully()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Test Menu Entry",
                Url = "https://example.com",
                IsEnabled = true
            };

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((MenuEntry)null);

            _ = this.mockMenuEntryRepository
                .Setup(x => x.InsertAsync(It.IsAny<MenuEntry>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.menuEntryService.CreateMenuEntry(menuEntryDto);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Name.Should().Be(menuEntryDto.Name);
            _ = result.Url.Should().Be(menuEntryDto.Url);
            _ = result.IsExternal.Should().BeTrue();

            this.mockMenuEntryRepository.Verify(x => x.GetByNameAsync(menuEntryDto.Name), Times.Once);
            this.mockMenuEntryRepository.Verify(x => x.InsertAsync(It.IsAny<MenuEntry>()), Times.Once);
            this.mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task CreateMenuEntry_EmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "",
                Url = "https://example.com"
            };

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentException>(async () =>
                await this.menuEntryService.CreateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task CreateMenuEntry_NameTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = new string('a', 101), // 101 characters
                Url = "https://example.com"
            };

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentException>(async () =>
                await this.menuEntryService.CreateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task CreateMenuEntry_DuplicateName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Existing Entry",
                Url = "https://example.com"
            };

            var existingEntry = new MenuEntry
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Entry",
                Url = "https://existing.com"
            };

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByNameAsync(menuEntryDto.Name))
                .ReturnsAsync(existingEntry);

            // Act & Assert
            _ = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await this.menuEntryService.CreateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task CreateMenuEntry_EmptyUrl_ShouldThrowArgumentException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Test Entry",
                Url = ""
            };

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentException>(async () =>
                await this.menuEntryService.CreateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task CreateMenuEntry_InvalidUrlFormat_ShouldThrowArgumentException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Test Entry",
                Url = "not-a-valid-url"
            };

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((MenuEntry)null);

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentException>(async () =>
                await this.menuEntryService.CreateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task GetAllMenuEntries_ShouldReturnAllEntries()
        {
            // Arrange
            var menuEntries = Fixture.CreateMany<MenuEntry>(3).ToList();

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetAllAsync(null, default))
                .ReturnsAsync(menuEntries);

            // Act
            var result = await this.menuEntryService.GetAllMenuEntries();

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().HaveCount(3);

            this.mockMenuEntryRepository.Verify(x => x.GetAllAsync(null, default), Times.Once);
        }

        [Test]
        public async Task GetMenuEntryById_ExistingId_ShouldReturnEntry()
        {
            // Arrange
            var menuEntry = Fixture.Create<MenuEntry>();

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(menuEntry.Id))
                .ReturnsAsync(menuEntry);

            // Act
            var result = await this.menuEntryService.GetMenuEntryById(menuEntry.Id);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Id.Should().Be(menuEntry.Id);

            this.mockMenuEntryRepository.Verify(x => x.GetByIdAsync(menuEntry.Id), Times.Once);
        }

        [Test]
        public async Task GetMenuEntryById_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString();

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(nonExistingId))
                .ReturnsAsync((MenuEntry)null);

            // Act
            var result = await this.menuEntryService.GetMenuEntryById(nonExistingId);

            // Assert
            _ = result.Should().BeNull();

            this.mockMenuEntryRepository.Verify(x => x.GetByIdAsync(nonExistingId), Times.Once);
        }

        [Test]
        public async Task UpdateMenuEntry_ValidDto_ShouldUpdateSuccessfully()
        {
            // Arrange
            var existingEntry = Fixture.Create<MenuEntry>();
            var menuEntryDto = new MenuEntryDto
            {
                Id = existingEntry.Id,
                Name = "Updated Name",
                Url = "https://updated.com",
                IsEnabled = true
            };

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(menuEntryDto.Id))
                .ReturnsAsync(existingEntry);

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByNameAsync(menuEntryDto.Name))
                .ReturnsAsync((MenuEntry)null);

            _ = this.mockMenuEntryRepository
                .Setup(x => x.Update(It.IsAny<MenuEntry>()));

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.menuEntryService.UpdateMenuEntry(menuEntryDto);

            // Assert
            this.mockMenuEntryRepository.Verify(x => x.GetByIdAsync(menuEntryDto.Id), Times.Once);
            this.mockMenuEntryRepository.Verify(x => x.Update(It.IsAny<MenuEntry>()), Times.Once);
            this.mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateMenuEntry_NonExistingId_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Entry",
                Url = "https://example.com"
            };

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(menuEntryDto.Id))
                .ReturnsAsync((MenuEntry)null);

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.menuEntryService.UpdateMenuEntry(menuEntryDto));
        }

        [Test]
        public async Task DeleteMenuEntry_ExistingId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var menuEntry = Fixture.Create<MenuEntry>();

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(menuEntry.Id))
                .ReturnsAsync(menuEntry);

            _ = this.mockMenuEntryRepository
                .Setup(x => x.Delete(menuEntry.Id));

            _ = this.mockUnitOfWork
                .Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.menuEntryService.DeleteMenuEntry(menuEntry.Id);

            // Assert
            this.mockMenuEntryRepository.Verify(x => x.GetByIdAsync(menuEntry.Id), Times.Once);
            this.mockMenuEntryRepository.Verify(x => x.Delete(menuEntry.Id), Times.Once);
            this.mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteMenuEntry_NonExistingId_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString();

            _ = this.mockMenuEntryRepository
                .Setup(x => x.GetByIdAsync(nonExistingId))
                .ReturnsAsync((MenuEntry)null);

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.menuEntryService.DeleteMenuEntry(nonExistingId));
        }
    }
}
