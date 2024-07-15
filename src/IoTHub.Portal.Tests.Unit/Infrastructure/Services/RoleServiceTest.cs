// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using FluentAssertions;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class RoleServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IRoleRepository> mockRoleRepository;
        private Mock<IActionRepository> mockActionRepository;

        private IRoleManagementService roleService;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockRoleRepository = new Mock<IRoleRepository>();
            this.mockActionRepository = new Mock<IActionRepository>();
            this.mockActionRepository = new Mock<IActionRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockRoleRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockActionRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockActionRepository.Object);
            _ = ServiceCollection.AddSingleton<IRoleManagementService, RoleService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.roleService = Services.GetRequiredService<IRoleManagementService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetRoleShouldReturnAList()
        {
            //Arrange
            // Arrange
            var expectedRoles = Fixture.CreateMany<RoleModel>(3).ToList();
            var expectedRolesList = expectedRoles.Select(role => Mapper.Map<Role>(role)).ToList();

            var paginatedResult = new PaginatedResult<Role>(expectedRolesList, expectedRolesList.Count, 0, 10);

            var roleFilter = new RoleFilter
            {
                Keyword = new Guid().ToString(),
            };

            _ = this.mockRoleRepository.Setup(repository => repository.GetPaginatedListAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string[]>(),
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Role, object>>[]>()
            )).ReturnsAsync(paginatedResult);

            //Act
            var result = await this.roleService.GetRolePage(roleFilter.Keyword, roleFilter.PageSize, roleFilter.PageNumber, roleFilter.OrderBy);

            //Assert
            _ = result.Data.Should().BeEquivalentTo(expectedRoles);
            MockRepository.VerifyAll();

        }

        [Test]
        public async Task GetRoleShouldReturnExpectedValues()
        {
            // Arrange
            var expectedActions = Fixture.CreateMany<string>(2).ToList();

            var expectedRole = new RoleDetailsModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Color = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Actions = expectedActions
            };

            var expectedRoleEntity = Mapper.Map<Role>(expectedRole);

            _ = this.mockRoleRepository.Setup(repository => repository.GetByIdAsync(
                expectedRole.Id,
                It.IsAny<Expression<Func<Role, object>>[]>()
            )).ReturnsAsync(expectedRoleEntity);

            // Act
            var result = await this.roleService.GetRoleDetailsAsync(expectedRole.Id);

            // Assert
            Assert.IsNotNull(result);
            _ = result.Should().BeEquivalentTo(expectedRole, options => options
                .Excluding(r => r.Id) // Excluding Id because it is not mapped
                .ComparingByMembers<RoleDetailsModel>());
        }

        [Test]
        public void GetRoleShouldThrowResourceNotFoundExceptionIfRoleDoesNotExist()
        {

            // Arrange
            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), r => r.Actions))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.roleService.GetRoleDetailsAsync("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetRoleDetailsAsync_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.roleService.GetRoleDetailsAsync(invalidId));
        }

        [Test]
        public async Task CreateRoleShouldCreate()
        {
            // Arrange

            var roleModel = Fixture.Create<RoleDetailsModel>();

            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Role)null);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.roleService.CreateRole(roleModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void CreateRoleShouldThrowResourceAlreadyExistsExceptionIfRoleAlreadyExists()
        {
            // Arrange
            var roleModel = Fixture.Create<RoleDetailsModel>();

            var existingRole = new Role { Name = roleModel.Name };
            _ = this.mockRoleRepository.Setup(x => x.GetByNameAsync(roleModel.Name))
                .ReturnsAsync(existingRole);

            // Act
            Func<Task> act = async () => await this.roleService.CreateRole(roleModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceAlreadyExistsException>();
        }

        [Test]
        public void CreateRole_NullRole_ThrowsArgumentNullException()
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.roleService.CreateRole(null));
        }

        [Test]
        public async Task UpdateRoleShouldUpdateRole()
        {
            // Arrange

            var roleModel = Fixture.Create<RoleDetailsModel>();
            var roleEntity = Mapper.Map<Role>(roleModel);

            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), r => r.Actions))
                .ReturnsAsync(roleEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.roleService.UpdateRole(roleModel.Id, roleModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void UpdateRole_NullRole_ThrowsArgumentNullException()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.roleService.UpdateRole(validId, null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void UpdateRole_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Arrange
            var roleModel = Fixture.Create<RoleDetailsModel>();

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.roleService.UpdateRole(invalidId, roleModel));
        }

        [Test]
        public async Task DeleteRoleShouldDeleteRole()
        {
            // Arrange
            var roleModel = Fixture.Create<RoleDetailsModel>();
            var roleEntity = Mapper.Map<Role>(roleModel);

            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), r => r.Actions))
                .ReturnsAsync(roleEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.roleService.DeleteRole(roleModel.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void DeleteRole_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.roleService.DeleteRole(invalidId));
        }
    }
}
