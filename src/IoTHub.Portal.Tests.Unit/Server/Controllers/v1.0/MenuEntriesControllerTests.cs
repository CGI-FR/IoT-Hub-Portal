// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Controllers.V10;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MenuEntriesControllerTests
    {
        private MockRepository mockRepository;
        private Mock<IMenuEntryService> mockMenuEntryService;
        private MenuEntriesController controller;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockMenuEntryService = this.mockRepository.Create<IMenuEntryService>();
            this.controller = new MenuEntriesController(this.mockMenuEntryService.Object);
        }

        [Test]
        public async Task CreateMenuEntry_ValidDto_ShouldReturnCreatedResult()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Test Entry",
                Url = "https://example.com",
                IsEnabled = true
            };

            var createdEntry = new MenuEntryDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = menuEntryDto.Name,
                Url = menuEntryDto.Url,
                IsEnabled = menuEntryDto.IsEnabled
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.CreateMenuEntry(It.IsAny<MenuEntryDto>()))
                .ReturnsAsync(createdEntry);

            // Act
            var result = await this.controller.CreateMenuEntry(menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.IsNotNull(createdResult.Value);
            var returnedEntry = createdResult.Value as MenuEntryDto;
            Assert.AreEqual(createdEntry.Id, returnedEntry.Id);
            Assert.AreEqual(createdEntry.Name, returnedEntry.Name);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateMenuEntry_InvalidDto_ShouldReturnBadRequest()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "",
                Url = "https://example.com"
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.CreateMenuEntry(It.IsAny<MenuEntryDto>()))
                .ThrowsAsync(new ArgumentException("Menu entry name is required"));

            // Act
            var result = await this.controller.CreateMenuEntry(menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badRequestResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateMenuEntry_DuplicateName_ShouldReturnConflict()
        {
            // Arrange
            var menuEntryDto = new MenuEntryDto
            {
                Name = "Existing Entry",
                Url = "https://example.com"
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.CreateMenuEntry(It.IsAny<MenuEntryDto>()))
                .ThrowsAsync(new InvalidOperationException("A menu entry with this name already exists"));

            // Act
            var result = await this.controller.CreateMenuEntry(menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ConflictObjectResult>(result);
            var conflictResult = result as ConflictObjectResult;
            Assert.AreEqual(409, conflictResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllMenuEntries_ShouldReturnOkWithList()
        {
            // Arrange
            var menuEntries = new List<MenuEntryDto>
            {
                new MenuEntryDto { Id = Guid.NewGuid().ToString(), Name = "Entry 1", Url = "https://example1.com" },
                new MenuEntryDto { Id = Guid.NewGuid().ToString(), Name = "Entry 2", Url = "https://example2.com" },
                new MenuEntryDto { Id = Guid.NewGuid().ToString(), Name = "Entry 3", Url = "https://example3.com" }
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.GetAllMenuEntries())
                .ReturnsAsync(menuEntries);

            // Act
            var result = await this.controller.GetAllMenuEntries();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedEntries = okResult.Value as IEnumerable<MenuEntryDto>;
            Assert.AreEqual(3, returnedEntries.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetMenuEntry_ExistingId_ShouldReturnOk()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();
            var menuEntry = new MenuEntryDto
            {
                Id = menuEntryId,
                Name = "Test Entry",
                Url = "https://example.com"
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.GetMenuEntryById(menuEntryId))
                .ReturnsAsync(menuEntry);

            // Act
            var result = await this.controller.GetMenuEntry(menuEntryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedEntry = okResult.Value as MenuEntryDto;
            Assert.AreEqual(menuEntryId, returnedEntry.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetMenuEntry_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString();

            _ = this.mockMenuEntryService
                .Setup(x => x.GetMenuEntryById(nonExistingId))
                .ReturnsAsync((MenuEntryDto)null);

            // Act
            var result = await this.controller.GetMenuEntry(nonExistingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateMenuEntry_ValidDto_ShouldReturnNoContent()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();
            var menuEntryDto = new MenuEntryDto
            {
                Id = menuEntryId,
                Name = "Updated Entry",
                Url = "https://updated.com"
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.UpdateMenuEntry(It.IsAny<MenuEntryDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.controller.UpdateMenuEntry(menuEntryId, menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NoContentResult>(result);
            var noContentResult = result as NoContentResult;
            Assert.AreEqual(204, noContentResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateMenuEntry_MismatchedId_ShouldReturnBadRequest()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();
            var differentId = Guid.NewGuid().ToString();
            var menuEntryDto = new MenuEntryDto
            {
                Id = differentId,
                Name = "Updated Entry",
                Url = "https://updated.com"
            };

            // Act
            var result = await this.controller.UpdateMenuEntry(menuEntryId, menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badRequestResult.StatusCode);

            // Verify service was never called
            this.mockMenuEntryService.Verify(x => x.UpdateMenuEntry(It.IsAny<MenuEntryDto>()), Times.Never);
        }

        [Test]
        public async Task UpdateMenuEntry_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();
            var menuEntryDto = new MenuEntryDto
            {
                Id = menuEntryId,
                Name = "Updated Entry",
                Url = "https://updated.com"
            };

            _ = this.mockMenuEntryService
                .Setup(x => x.UpdateMenuEntry(It.IsAny<MenuEntryDto>()))
                .ThrowsAsync(new ResourceNotFoundException("Menu entry not found"));

            // Act
            var result = await this.controller.UpdateMenuEntry(menuEntryId, menuEntryDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteMenuEntry_ExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();

            _ = this.mockMenuEntryService
                .Setup(x => x.DeleteMenuEntry(menuEntryId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.controller.DeleteMenuEntry(menuEntryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NoContentResult>(result);
            var noContentResult = result as NoContentResult;
            Assert.AreEqual(204, noContentResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteMenuEntry_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString();

            _ = this.mockMenuEntryService
                .Setup(x => x.DeleteMenuEntry(nonExistingId))
                .ThrowsAsync(new ResourceNotFoundException("Menu entry not found"));

            // Act
            var result = await this.controller.DeleteMenuEntry(nonExistingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, notFoundResult.StatusCode);

            this.mockRepository.VerifyAll();
        }
    }
}
