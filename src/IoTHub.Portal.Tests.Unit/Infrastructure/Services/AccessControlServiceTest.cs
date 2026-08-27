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
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
#pragma warning disable IDE0005
    using DomainAction = Portal.Domain.Entities.Action;
#pragma warning restore IDE0005

    public class AccessControlServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAccessControlRepository> mockAccessControlRepository;
        private Mock<IPrincipalRepository> mockPrincipalRepository;
        private Mock<IRoleRepository> mockRoleRepository;

        private IAccessControlManagementService accessControlService;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockAccessControlRepository = new Mock<IAccessControlRepository>();
            this.mockRoleRepository = new Mock<IRoleRepository>();
            this.mockPrincipalRepository = new Mock<IPrincipalRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockAccessControlRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockRoleRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockPrincipalRepository.Object);
            _ = ServiceCollection.AddSingleton<IAccessControlManagementService, AccessControlService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.accessControlService = Services.GetRequiredService<IAccessControlManagementService>();
            Mapper = Services.GetRequiredService<IMapper>();

            // Configure AutoFixture to avoid circular references
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task GetAccessControlPageShouldReturnAList()
        {
            // Arrange
            var role = Fixture.Create<Role>();
            var principal = Fixture.Create<Principal>();

            var accessControls = Fixture.Build<AccessControl>()
                .With(ac => ac.Role, role)
                .With(ac => ac.Principal, principal)
                .CreateMany(3)
                .ToList();

            var accessControlModelListItems = accessControls.Select(model => Mapper.Map<AccessControlModel>(model)).ToList();

            var paginatedResult = new PaginatedResult<AccessControl>(accessControls, accessControlModelListItems.Count, 0, 10);

            _ = this.mockAccessControlRepository.Setup(repository => repository.GetPaginatedListAsync(
                 It.IsAny<int>(),
                 It.IsAny<int>(),
                 It.IsAny<string[]>(),
                 It.IsAny<Expression<Func<AccessControl, bool>>>(),
                 It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<AccessControl, object>>[]>()
             )).ReturnsAsync(paginatedResult);

            // Act
            var result = await this.accessControlService.GetAccessControlPage();

            // Assert
            _ = result.Data.Should().BeEquivalentTo(accessControlModelListItems);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAccessControlShouldReturnExpectedValues()
        {
            var role = new RoleModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Color = Guid.NewGuid().ToString()
            };
            // Arrange
            var expectedAccessControl = new AccessControlModel()
            {
                Id = Guid.NewGuid().ToString(),
                Scope = Guid.NewGuid().ToString(),
                PrincipalId = Guid.NewGuid().ToString(),
                Role = role
            };

            var accessControlEntity = Mapper.Map<AccessControl>(expectedAccessControl);

            _ = this.mockAccessControlRepository.Setup(repository => repository.GetByIdAsync(
                expectedAccessControl.Id,
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            )).ReturnsAsync(accessControlEntity);

            // Act
            var result = await this.accessControlService.GetAccessControlAsync(expectedAccessControl.Id);

            // Assert
            Assert.IsNotNull(result);
            _ = result.Should().BeEquivalentTo(expectedAccessControl, options => options
                .Excluding(r => r.Id) // Excluding Id because it is not mapped
                .Excluding(r => r.Role)
                .ComparingByMembers<AccessControlModel>());
        }

        [Test]
        public void GetAccessControlShouldThrowResourceNotFoundExceptionIfAccessControlDoesNotExist()
        {

            // Arrange
            _ = this.mockAccessControlRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), ac => ac.Role))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.accessControlService.GetAccessControlAsync("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public async Task CreateRoleShouldCreate()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();

            var mockPrincipal = Fixture.Create<Principal>();
            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(mockPrincipal);

            var mockRole = Fixture.Create<Role>();
            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(accessControlModel.Role.Id))
                .ReturnsAsync(mockRole);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.accessControlService.CreateAccessControl(accessControlModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void CreateAccessControlAlreadyExistsShouldThrowResourceAlreadyExistsException()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();

            var existingAccessControl = new AccessControl { Id = accessControlModel.Id };
            _ = this.mockAccessControlRepository.Setup(x => x.GetByIdAsync(accessControlModel.Id))
                .ReturnsAsync(existingAccessControl);

            // Act
            Func<Task> act = async () => await this.accessControlService.CreateAccessControl(accessControlModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceAlreadyExistsException>();
        }

        [Test]
        public async Task DeleteAccessControlShouldDeleteIt()
        {
            // Arrange
            var accessControl = Fixture.Create<AccessControl>();
            var accessControlId = accessControl.Id;

            _ = mockAccessControlRepository.Setup(x => x.GetByIdAsync(accessControlId))
                .ReturnsAsync(accessControl);

            // Act
            _ = await accessControlService.DeleteAccessControl(accessControlId);

            // Assert
            mockAccessControlRepository.Verify(x => x.Delete(accessControlId), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetAccessControlDetailsAsync_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.accessControlService.GetAccessControlAsync(invalidId));
        }

        [Test]
        public void CreateAccessControl_NulAccessControl_ThrowsArgumentNullException()
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.accessControlService.CreateAccessControl(null));
        }

        [Test]
        public void UpdateAccessControWithANullAccessControl_ThrowsArgumentNullException()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.accessControlService.UpdateAccessControl(validId, null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void UpdateAccessControl_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.accessControlService.UpdateAccessControl(invalidId, accessControlModel));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void DeleteAccessControl_InvalidId_ThrowsArgumentException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.accessControlService.DeleteAccessControl(invalidId));
        }

        [Test]
        public async Task UserHasPermissionAsync_ShouldReturnTrue_WhenPermissionExists()
        {
            // Arrange
            var principalId = "principal-123";
            var permission = "access-control:read";
            var matchingAction = new DomainAction { Name = permission };
            var role = new Role { Actions = new List<DomainAction> { matchingAction } };
            var accessControl = new AccessControl { PrincipalId = principalId, Role = role };

            _ = this.mockAccessControlRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<AccessControl, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            ))
            .ReturnsAsync(new List<AccessControl> { accessControl });

            // Act
            var result = await accessControlService.UserHasPermissionAsync(principalId, permission);

            // Assert
            _ = result.Should().BeTrue();
        }

        [Test]
        public async Task UserHasPermissionAsync_ShouldReturnFalse_WhenPermissionDoesNotExist()
        {
            // Arrange
            var principalId = "principal-456";
            var permission = "access-control:write";
            // Here the action does not match the requested permission.
            var nonMatchingAction = new DomainAction { Name = "another-action" };
            var role = new Role { Actions = new List<DomainAction> { nonMatchingAction } };
            var accessControl = new AccessControl { PrincipalId = principalId, Role = role };

            _ = this.mockAccessControlRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<AccessControl, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            ))
            .ReturnsAsync(new List<AccessControl> { accessControl });

            // Act
            var result = await accessControlService.UserHasPermissionAsync(principalId, permission);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Test]
        public void CreateAccessControl_PrincipalNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();

            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(value: null);

            // Act
            Func<Task> act = async () => await this.accessControlService.CreateAccessControl(accessControlModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public void CreateAccessControl_RoleNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();

            var mockPrincipal = Fixture.Create<Principal>();
            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(mockPrincipal);

            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(accessControlModel.Role.Id))
                .ReturnsAsync(value: null);

            // Act
            Func<Task> act = async () => await this.accessControlService.CreateAccessControl(accessControlModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public async Task UpdateAccessControlShouldUpdate()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();
            var existingAccessControl = Mapper.Map<AccessControl>(accessControlModel);

            _ = this.mockAccessControlRepository.Setup(x => x.GetByIdAsync(
                accessControlModel.Id,
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            )).ReturnsAsync(existingAccessControl);

            var mockPrincipal = Fixture.Create<Principal>();
            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(mockPrincipal);

            var mockRole = Fixture.Create<Role>();
            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(accessControlModel.Role.Id))
                .ReturnsAsync(mockRole);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.accessControlService.UpdateAccessControl(accessControlModel.Id, accessControlModel);

            // Assert
            Assert.IsNotNull(result);
            this.mockAccessControlRepository.Verify(x => x.Update(existingAccessControl), Times.Once);
            this.mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public void UpdateAccessControl_PrincipalNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();
            var existingAccessControl = Mapper.Map<AccessControl>(accessControlModel);

            _ = this.mockAccessControlRepository.Setup(x => x.GetByIdAsync(
                accessControlModel.Id,
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            )).ReturnsAsync(existingAccessControl);

            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(value: null);

            // Act
            Func<Task> act = async () => await this.accessControlService.UpdateAccessControl(accessControlModel.Id, accessControlModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public void UpdateAccessControl_RoleNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var accessControlModel = Fixture.Create<AccessControlModel>();
            var existingAccessControl = Mapper.Map<AccessControl>(accessControlModel);

            _ = this.mockAccessControlRepository.Setup(x => x.GetByIdAsync(
                accessControlModel.Id,
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            )).ReturnsAsync(existingAccessControl);

            var mockPrincipal = Fixture.Create<Principal>();
            _ = this.mockPrincipalRepository.Setup(x => x.GetByIdAsync(accessControlModel.PrincipalId))
                .ReturnsAsync(mockPrincipal);

            _ = this.mockRoleRepository.Setup(x => x.GetByIdAsync(accessControlModel.Role.Id))
                .ReturnsAsync(value: null);

            // Act
            Func<Task> act = async () => await this.accessControlService.UpdateAccessControl(accessControlModel.Id, accessControlModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        public async Task GetUserPermissionsAsyncShouldReturnDistinctPermissions()
        {
            // Arrange
            var principalId = "principal-789";
            var actionOne = new DomainAction { Name = "device:read" };
            var actionTwo = new DomainAction { Name = "device:write" };
            var duplicateAction = new DomainAction { Name = "device:read" };

            var roleOne = new Role { Actions = new List<DomainAction> { actionOne, actionTwo } };
            var roleTwo = new Role { Actions = new List<DomainAction> { duplicateAction } };

            var accessControls = new List<AccessControl>
            {
                new AccessControl { PrincipalId = principalId, Role = roleOne },
                new AccessControl { PrincipalId = principalId, Role = roleTwo }
            };

            _ = this.mockAccessControlRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<AccessControl, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            ))
            .ReturnsAsync(accessControls);

            // Act
            var result = await this.accessControlService.GetUserPermissionsAsync(principalId);

            // Assert
            _ = result.Should().BeEquivalentTo(new[] { "device:read", "device:write" });
        }

        [Test]
        public async Task GetUserPermissionsAsyncShouldReturnEmptyWhenNoAccessControls()
        {
            // Arrange
            var principalId = "principal-none";

            _ = this.mockAccessControlRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<AccessControl, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<AccessControl, object>>[]>()
            ))
            .ReturnsAsync(new List<AccessControl>());

            // Act
            var result = await this.accessControlService.GetUserPermissionsAsync(principalId);

            // Assert
            _ = result.Should().BeEmpty();
        }
    }
}
