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

    public class UserServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IPrincipalRepository> mockPrincipalRepository;

        private IUserManagementService userService;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockPrincipalRepository = new Mock<IPrincipalRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockUserRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockPrincipalRepository.Object);
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
    }
}
