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
    public class UserssControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<UsersController>> mockLogger;
        private Mock<IUserManagementService> mockUserService;
        private Mock<IAccessControlManagementService> mockAccessControlService;
        private Mock<IUrlHelper> mockUrlHelper;
        private IDataProtectionProvider mockDataProtectionProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<UsersController>>();
            this.mockUserService = this.mockRepository.Create<IUserManagementService>();
            this.mockAccessControlService = this.mockRepository.Create<IAccessControlManagementService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDataProtectionProvider = new EphemeralDataProtectionProvider();
        }

        private UsersController CreateUsersController()
        {
            return new UsersController(
                this.mockUserService.Object,
                this.mockLogger.Object,
                this.mockAccessControlService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllUsersReturnOkResult()
        {
            // Arrange
            var userController = CreateUsersController();

            var paginedUsers = new PaginatedResult<UserModel>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new UserModel()
                {
                    Id = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockUserService
                .Setup(x => x.GetUserPage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(paginedUsers);

            var locationUrl = "http://location/users";

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
            var result = await userController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(paginedUsers.Data.Count, result.Items.Count());

            this.mockRepository.VerifyAll();
        }


        [Test]
        public async Task GetByIdShouldReturnTheCorrespondantUser()
        {
            // Arrange
            var usersController = CreateUsersController();

            var userId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.IsAny<It.IsAnyType>(),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
             ));
            _ = this.mockUserService
                .Setup(x => x.GetUserPage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(new PaginatedResult<UserModel>()
                {
                    Data = new List<UserModel>() { new UserModel() { Id = userId } },
                    TotalCount = 1,
                    PageSize = 10,
                    CurrentPage = 0
                });

            // Act
            var result = await usersController.Get(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<PaginationResult<UserModel>>(result);

            var user = result.Items.First();

            Assert.IsNotNull(user);
            Assert.AreEqual(userId, user.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateUserShouldReturnOk()
        {
            // Arrange
            var usersController = CreateUsersController();

            var user = new UserDetailsModel()
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

            _ = this.mockUserService
                .Setup(x => x.CreateUserAsync(It.Is<UserDetailsModel>(c => c.Id.Equals(user.Id, StringComparison.Ordinal))))
                .ReturnsAsync(user);

            // Act
            var result = await usersController.CreateUser(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<CreatedAtActionResult>(result); // Change this line

            var createdAtActionResult = result as CreatedAtActionResult;

            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);

            Assert.IsNotNull(createdAtActionResult.Value);
            Assert.IsAssignableFrom<UserDetailsModel>(createdAtActionResult.Value);

            var userObj = createdAtActionResult.Value as UserDetailsModel;
            Assert.IsNotNull(userObj);
            Assert.AreEqual(user.Id, userObj.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateUserShouldReturnOkResult()
        {
            // Arrange
            var usersController = CreateUsersController();


            var userId = Guid.NewGuid().ToString();

            var user = new UserDetailsModel()
            {
                Id = userId //test
            };

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockUserService
                .Setup(x => x.UpdateUser(userId, It.Is<UserDetailsModel>(c => c.Id.Equals(user.Id, StringComparison.Ordinal))))
                .ReturnsAsync(user);

            // Act
            var result = await usersController.EditUserAsync(userId, user);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        /*[Test]
        public async Task DeleteUserShouldReturnExpectedBehavior()
        {
            // Arrange
            var usersController = CreateUsersController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockUserService.Setup(c => c.DeleteUser(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(true);

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await usersController.DeleteUser(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }*/
    }
}
