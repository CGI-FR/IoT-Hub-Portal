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
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v1._0;
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

            // Add this line to setup LogInformation method
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
            Assert.IsAssignableFrom<CreatedAtActionResult>(result); // Change this line

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
        public async Task UpdateGroupShouldReturnOkResult()
        {
            // Arrange
            var groupsController = CreateGroupsController();


            var groupId = Guid.NewGuid().ToString();

            var group = new GroupDetailsModel()
            {
                Id = groupId //test
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

        /*[Test]
        public async Task DeleteGroupShouldReturnExpectedBehavior()
        {
            // Arrange
            var groupsController = CreateGroupsController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockGroupService.Setup(c => c.DeleteGroup(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(true);

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await groupsController.DeleteGroup(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }*/
    }
}