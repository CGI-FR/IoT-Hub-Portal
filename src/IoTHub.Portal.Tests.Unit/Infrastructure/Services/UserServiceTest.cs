// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
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

    public class UserServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IPrincipalRepository> mockPrincipalRepository;
        private Mock<IAccessControlRepository> mockAccessControlRepository;
        private Mock<IRoleRepository> mockRoleRepository;
        private IUserManagementService userService;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            Fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockPrincipalRepository = new Mock<IPrincipalRepository>();
            this.mockAccessControlRepository = new Mock<IAccessControlRepository>();
            this.mockRoleRepository = new Mock<IRoleRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockUserRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockPrincipalRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockAccessControlRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockRoleRepository.Object);
            _ = ServiceCollection.AddSingleton<IUserManagementService, UserService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.userService = Services.GetRequiredService<IUserManagementService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetUserShouldReturnAList()
        {
            //Arrange
            // Arrange
            var expectedUsers = Fixture.CreateMany<UserModel>(3).ToList();
            var expectedUsersList = expectedUsers.Select(user => Mapper.Map<User>(user)).ToList();

            var paginatedResult = new PaginatedResult<User>(expectedUsersList, expectedUsersList.Count, 0, 10);

            _ = this.mockUserRepository.Setup(repository => repository.GetPaginatedListAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string[]>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync(paginatedResult);

            //Act
            var result = await this.userService.GetUserPage();

            //Assert
            _ = result.Data.Should().BeEquivalentTo(expectedUsers, options => options.Excluding(user => user.Id));
            MockRepository.VerifyAll();

        }

        [Test]
        public async Task GetUserShouldReturnExpectedValues()
        {
            // Arrange
            var expectedActions = Fixture.CreateMany<string>(2).ToList();

            var expectedUser = new UserDetailsModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Email = Guid.NewGuid().ToString(),
                GivenName = Guid.NewGuid().ToString(),
                FamilyName = Guid.NewGuid().ToString(),
                Avatar = Guid.NewGuid().ToString(),
            };

            var expectedUserEntity = Mapper.Map<User>(expectedUser);

            _ = this.mockUserRepository.Setup(repository => repository.GetByIdAsync(
                expectedUser.Id,
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync(expectedUserEntity);

            // Act
            var result = await this.userService.GetUserDetailsAsync(expectedUser.Id);

            // Assert
            Assert.IsNotNull(result);
            _ = result.Should().BeEquivalentTo(expectedUser, options => options
                .Excluding(r => r.Id) // Excluding Id because it is not mapped
                .ComparingByMembers<UserDetailsModel>());
        }

        [Test]
        public void GetUserShouldThrowResourceNotFoundExceptionIfUserDoesNotExist()
        {

            // Arrange
            _ = this.mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.userService.GetUserDetailsAsync("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetUserDetailsAsyncInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.userService.GetUserDetailsAsync(invalidId));
        }

        [Test]
        public async Task CreateUserShouldCreate()
        {
            // Arrange

            var userModel = Fixture.Create<UserDetailsModel>();

            _ = this.mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.userService.CreateUserAsync(userModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void CreateUserShouldThrowResourceAlreadyExistsExceptionIfUserAlreadyExists()
        {
            // Arrange
            var userModel = Fixture.Create<UserDetailsModel>();

            var existingUser = new User { Name = userModel.Name };
            _ = this.mockUserRepository.Setup(x => x.GetByNameAsync(userModel.Name))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await this.userService.CreateUserAsync(userModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceAlreadyExistsException>();
        }

        [Test]
        public void CreateUserWithNullUserThrowsArgumentNullException()
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.userService.CreateUserAsync(null));
        }

        [Test]
        public async Task UpdateUserShouldUpdateUser()
        {
            // Arrange

            var userModel = Fixture.Create<UserDetailsModel>();
            var userEntity = Mapper.Map<User>(userModel);

            _ = this.mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.userService.UpdateUser(userModel.Id, userModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void UpdateUserWithNullUserThrowsArgumentNullException()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.userService.UpdateUser(validId, null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]

        [TestCase(" ")]
        public void UpdateUserWithInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Arrange
            var userModel = Fixture.Create<UserDetailsModel>();

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.userService.UpdateUser(invalidId, userModel));
        }

        [Test]
        public async Task DeleteUserShouldDeleteUser()
        {
            // Arrange
            var userModel = Fixture.Create<UserDetailsModel>();
            var userEntity = Mapper.Map<User>(userModel);

            _ = this.mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.userService.DeleteUser(userModel.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void DeleteUserWithInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.userService.DeleteUser(invalidId));
        }

        [Test]
        public async Task GetOrCreateUserByEmailAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "new@example.com";
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new []
            {
                new Claim("name", "New User"),
                new Claim("preferred_username", "newuser"),
                new Claim("family_name", "User")
            }, "TestAuth"));

            // No existing user for this email
            _ = this.mockUserRepository
                 .Setup(r => r.GetAllAsync(
                     It.IsAny<Expression<Func<User, bool>>>(),
                     It.IsAny<CancellationToken>(),
                     It.IsAny<Expression<Func<User, object>>[]>()
                 ))
                 .ReturnsAsync(new List<User>());

            // We suppose that the insertion succeeds
            _ = this.mockUserRepository
                 .Setup(r => r.InsertAsync(It.Is<User>(u => u.Email.ToLower() == email.ToLower())))
                 .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(uow => uow.SaveAsync()).Returns(Task.CompletedTask);

            // We assume that the new user is created and we can retrieve it
            var newUser = Fixture.Build<User>()
                .With(u => u.Id, Guid.NewGuid().ToString())
                .With(u => u.Email, email)
                .With(u => u.Name, "New User")
                .With(u => u.GivenName, "newuser")
                .With(u => u.FamilyName, "User")
                .With(u => u.PrincipalId, Guid.NewGuid().ToString())
                .Create();
            _ = this.mockUserRepository
                 .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<Expression<Func<User, object>>[]>()))
                 .ReturnsAsync(newUser);

            var expectedModel = new UserDetailsModel
            {
                Id = newUser.Id,
                Email = newUser.Email,
                Name = newUser.Name,
                GivenName = newUser.GivenName,
                FamilyName = newUser.FamilyName,
                PrincipalId = newUser.PrincipalId
            };

            // Add default Administrators role
            var adminRole = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Administrators"
            };
            _ = this.mockRoleRepository
                 .Setup(r => r.GetByNameAsync("Administrators"))
                 .ReturnsAsync(adminRole);

            // Act
            var result = await userService.GetOrCreateUserByEmailAsync(email, principal);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedModel,
                opts => opts.Excluding(obj => obj.PrincipalId).Excluding(obj => obj.Id));
            this.mockUserRepository.Verify(r => r.InsertAsync(It.Is<User>(u => u.Email.ToLower() == email.ToLower())), Times.Once);
            this.mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Exactly(2));
        }
    }
}
