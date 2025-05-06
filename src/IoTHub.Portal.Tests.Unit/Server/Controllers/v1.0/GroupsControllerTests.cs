// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Controllers.V10;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System.Linq;
    using System;

    [TestFixture]
    public class GroupssControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<GroupsController>> mockLogger;
        private Mock<IGroupManagementService> mockGroupService;
        private Mock<IAccessControlManagementService> mockAccessControlService;
        private Mock<IUrlHelper> mockUrlHelper;
        private IDataProtectionProvider mockDataProtectionProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<GroupsController>>();
            this.mockGroupService = this.mockRepository.Create<IGroupManagementService>();
            this.mockAccessControlService = this.mockRepository.Create<IAccessControlManagementService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDataProtectionProvider = new EphemeralDataProtectionProvider();
        }

        private GroupsController CreateGroupsController()
        {
            return new GroupsController(
                this.mockGroupService.Object,
                this.mockLogger.Object,
                this.mockAccessControlService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllGroupsReturnOkResult()
        {
            // Arrange
            var groupController = CreateGroupsController();

            var paginedGroups = new PaginatedResult<GroupModel>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new GroupModel()
                {
                    Id = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockGroupService
                .Setup(x => x.GetGroupPage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(paginedGroups);

            var locationUrl = "http://location/groups";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));



            // Act
            var result = await groupController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(paginedGroups.Data.Count, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Get_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var groupsController = CreateGroupsController();

            _ = this.mockGroupService
                .Setup(x => x.GetGroupPage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await groupsController.Get());

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetByIdShouldReturnTheCorrespondantGroup()
        {
            // Arrange
            var groupsController = CreateGroupsController();

            var groupId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.IsAny<It.IsAnyType>(),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
             ));
            _ = this.mockGroupService
                .Setup(x => x.GetGroupPage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(new PaginatedResult<GroupModel>()
                {
                    Data = new List<GroupModel>() { new GroupModel() { Id = groupId } },
                    TotalCount = 1,
                    PageSize = 10,
                    CurrentPage = 0
                });

            // Act
            var result = await groupsController.Get(groupId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<PaginationResult<GroupModel>>(result);

            var group = result.Items.First();

            Assert.IsNotNull(group);
            Assert.AreEqual(groupId, group.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetGroupDetailsShouldReturnNotFoundForNonExistingGroup()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var groupId = Guid.NewGuid().ToString();

            _ = this.mockGroupService
                .Setup(x => x.GetGroupDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((GroupDetailsModel)null);

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await groupsController.GetGroupDetails(groupId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateGroupShouldReturnOk()
        {
            // Arrange
            var groupsController = CreateGroupsController();

            var group = new GroupDetailsModel()
            {
                Id = Guid.NewGuid().ToString()
            };

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockGroupService
                .Setup(x => x.CreateGroupAsync(It.Is<GroupDetailsModel>(c => c.Id.Equals(group.Id, StringComparison.Ordinal))))
                .ReturnsAsync(group);

            // Act
            var result = await groupsController.CreateGroup(group);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            var createdAtActionResult = result as CreatedAtActionResult;

            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);

            Assert.IsNotNull(createdAtActionResult.Value);
            Assert.IsAssignableFrom<GroupDetailsModel>(createdAtActionResult.Value);

            var groupObj = createdAtActionResult.Value as GroupDetailsModel;
            Assert.IsNotNull(groupObj);
            Assert.AreEqual(group.Id, groupObj.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateGroupShouldReturnBadRequestWhenCreationFails()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var group = new GroupDetailsModel()
            {
                Id = Guid.NewGuid().ToString()
            };

            _ = this.mockGroupService
                .Setup(x => x.CreateGroupAsync(It.IsAny<GroupDetailsModel>()))
                .Throws(new Exception());

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await groupsController.CreateGroup(group);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateGroupShouldReturnOkResult()
        {
            // Arrange
            var groupsController = CreateGroupsController();


            var groupId = Guid.NewGuid().ToString();

            var group = new GroupDetailsModel()
            {
                Id = groupId
            };

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockGroupService
                .Setup(x => x.UpdateGroup(groupId, It.Is<GroupDetailsModel>(c => c.Id.Equals(group.Id, StringComparison.Ordinal))))
                .ReturnsAsync(group);

            // Act
            var result = await groupsController.EditGroupAsync(groupId, group);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task EditGroupAsync_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var groupId = Guid.NewGuid().ToString();
            var group = new GroupDetailsModel() { Id = groupId };

            _ = this.mockGroupService
                .Setup(x => x.UpdateGroup(groupId, It.IsAny<GroupDetailsModel>()))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await groupsController.EditGroupAsync(groupId, group));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteGroupShouldReturnExpectedBehavior()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockGroupService.Setup(c => c.DeleteGroup(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(true);

            _ = this.mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await groupsController.DeleteGroup(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteGroup_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var groupId = Guid.NewGuid().ToString();

            _ = this.mockGroupService.Setup(c => c.DeleteGroup(It.Is<string>(x => x == groupId)))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await groupsController.DeleteGroup(groupId));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetGroupDetails_ShouldMergeAccessControls_WhenFound()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var groupId = "group-123";
            var groupDetails = new GroupDetailsModel
            {
                Id = groupId,
                PrincipalId = "principal-group",
                AccessControls = new List<AccessControlModel>()
            };

            var accessControls = new PaginatedResult<AccessControlModel>
            {
                Data = new List<AccessControlModel>
                {
                    new AccessControlModel { Id = "ac-1" },
                    new AccessControlModel { Id = "ac-2" }
                },
                TotalCount = 2,
                PageSize = 100,
                CurrentPage = 0
            };

            // We configure the group service to return the details
            _ = this.mockGroupService.Setup(s => s.GetGroupDetailsAsync(It.IsAny<string>())).ReturnsAsync(groupDetails);
            // We configure the access control service to return a page with the access
            _ = this.mockAccessControlService.Setup(s => s.GetAccessControlPage(null, 100, 0, null, groupDetails.PrincipalId))
                .ReturnsAsync(accessControls);
            _ = this.mockLogger.Setup(x => x.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               null,
               It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var actionResult = await groupsController.GetGroupDetails(groupId);
            var okResult = actionResult as OkObjectResult;
            var returnedGroup = okResult.Value as GroupDetailsModel;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(returnedGroup);
            Assert.AreEqual(2, returnedGroup.AccessControls.Count);
            this.mockRepository.VerifyAll();
        }
    }
}
