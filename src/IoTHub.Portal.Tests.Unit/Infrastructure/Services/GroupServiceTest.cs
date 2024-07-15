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

    public class GroupServiceTest : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IGroupRepository> mockGroupRepository;
        private Mock<IPrincipalRepository> mockPrincipalRepository;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IAccessControlRepository> mockAccessControlRepository;

        private IGroupManagementService groupService;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockGroupRepository = new Mock<IGroupRepository>();
            this.mockPrincipalRepository = new Mock<IPrincipalRepository>();
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockAccessControlRepository = new Mock<IAccessControlRepository>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockGroupRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockPrincipalRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUserRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockAccessControlRepository.Object);
            _ = ServiceCollection.AddSingleton<IGroupManagementService, GroupService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.groupService = Services.GetRequiredService<IGroupManagementService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetGroupShouldReturnAList()
        {
            //Arrange
            // Arrange
            var expectedGroups = Fixture.CreateMany<GroupModel>(3).ToList();
            var expectedGroupsList = expectedGroups.Select(group => Mapper.Map<Group>(group)).ToList();

            var paginatedResult = new PaginatedResult<Group>(expectedGroupsList, expectedGroupsList.Count, 0, 10);

            _ = this.mockGroupRepository.Setup(repository => repository.GetPaginatedListAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string[]>(),
                It.IsAny<Expression<Func<Group, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Group, object>>[]>()
            )).ReturnsAsync(paginatedResult);

            //Act
            var result = await this.groupService.GetGroupPage();

            //Assert
            _ = result.Data.Should().BeEquivalentTo(expectedGroups, options => options.Excluding(group => group.Id));
            MockRepository.VerifyAll();

        }

        [Test]
        public async Task GetGroupShouldReturnExpectedValues()
        {
            // Arrange
            var expectedActions = Fixture.CreateMany<string>(2).ToList();

            var expectedGroup = new GroupDetailsModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Color = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString()
            };

            var expectedGroupEntity = Mapper.Map<Group>(expectedGroup);

            _ = this.mockGroupRepository.Setup(repository => repository.GetByIdAsync(
                expectedGroup.Id,
                It.IsAny<Expression<Func<Group, object>>[]>()
            )).ReturnsAsync(expectedGroupEntity);

            // Act
            var result = await this.groupService.GetGroupDetailsAsync(expectedGroup.Id);

            // Assert
            Assert.IsNotNull(result);
            _ = result.Should().BeEquivalentTo(expectedGroup, options => options
                .Excluding(r => r.Id) // Excluding Id because it is not mapped
                .ComparingByMembers<GroupDetailsModel>());
        }

        [Test]
        public void GetGroupShouldThrowResourceNotFoundExceptionIfGroupDoesNotExist()
        {

            // Arrange
            _ = this.mockGroupRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(value: null);

            // Act
            var result = async () => await this.groupService.GetGroupDetailsAsync("test");

            // Assert
            _ = result.Should().ThrowAsync<ResourceNotFoundException>();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetGroupDetailsAsyncInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.groupService.GetGroupDetailsAsync(invalidId));
        }

        [Test]
        public async Task CreateGroupShouldCreate()
        {
            // Arrange

            var groupModel = Fixture.Create<GroupDetailsModel>();

            _ = this.mockGroupRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Group)null);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.groupService.CreateGroupAsync(groupModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void CreateGroupShouldThrowResourceAlreadyExistsExceptionIfGroupAlreadyExists()
        {
            // Arrange
            var groupModel = Fixture.Create<GroupDetailsModel>();

            var existingGroup = new Group { Name = groupModel.Name };
            _ = this.mockGroupRepository.Setup(x => x.GetByNameAsync(groupModel.Name))
                .ReturnsAsync(existingGroup);

            // Act
            Func<Task> act = async () => await this.groupService.CreateGroupAsync(groupModel);

            // Assert
            _ = act.Should().ThrowAsync<ResourceAlreadyExistsException>();
        }

        [Test]
        public void CreateGroupWithNullGroupThrowsArgumentNullException()
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.groupService.CreateGroupAsync(null));
        }

        [Test]
        public async Task UpdateGroupShouldUpdateGroup()
        {
            // Arrange

            var groupModel = Fixture.Create<GroupDetailsModel>();
            var groupEntity = Mapper.Map<Group>(groupModel);

            _ = this.mockGroupRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(groupEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.groupService.UpdateGroup(groupModel.Id, groupModel);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void UpdateGroupWithNullGroupThrowsArgumentNullException()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            // Act & Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.groupService.UpdateGroup(validId, null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]

        [TestCase(" ")]
        public void UpdateGroupWithInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Arrange
            var groupModel = Fixture.Create<GroupDetailsModel>();

            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.groupService.UpdateGroup(invalidId, groupModel));
        }

        [Test]
        public async Task DeleteGroupShouldDeleteGroup()
        {
            // Arrange
            var groupModel = Fixture.Create<GroupDetailsModel>();
            var groupEntity = Mapper.Map<Group>(groupModel);

            _ = this.mockGroupRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(groupEntity);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.groupService.DeleteGroup(groupModel.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void DeleteGroupWithInvalidIdThrowsResourceNotFoundException(string invalidId)
        {
            // Act & Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.groupService.DeleteGroup(invalidId));
        }
    }
}
