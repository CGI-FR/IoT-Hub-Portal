// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;

    [TestFixture]
    public class RolesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<RolesController>> mockLogger;
        private Mock<IRoleManagementService> mockRoleService;
        private Mock<IUrlHelper> mockUrlHelper;
        private IDataProtectionProvider mockDataProtectionProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<RolesController>>();
            this.mockRoleService = this.mockRepository.Create<IRoleManagementService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDataProtectionProvider = new EphemeralDataProtectionProvider();
        }

        private RolesController CreateRolesController()
        {
            return new RolesController(
                this.mockRoleService.Object,
                this.mockLogger.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllRolesReturnOkResult()
        {
            // Arrange
            var roleController = CreateRolesController();

            var paginedRoles = new PaginatedResult<RoleModel>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new RoleModel()
                {
                    Id = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockRoleService
                .Setup(x => x.GetRolePage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(paginedRoles);

            var locationUrl = "http://location/roles";

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
            var result = await roleController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(paginedRoles.Data.Count, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Get_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var accessControlController = CreateRolesController();

            _ = this.mockRoleService
                .Setup(x => x.GetRolePage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await accessControlController.Get());

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetByIdShouldReturnTheCorrespondantRole()
        {
            // Arrange
            var rolesController = CreateRolesController();

            var roleId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.IsAny<It.IsAnyType>(),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
             ));
            _ = this.mockRoleService
                .Setup(x => x.GetRolePage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>()
                ))
                .ReturnsAsync(new PaginatedResult<RoleModel>()
                {
                    Data = new List<RoleModel>() { new RoleModel() { Id = roleId } },
                    TotalCount = 1,
                    PageSize = 10,
                    CurrentPage = 0
                });

            // Act
            var result = await rolesController.Get(roleId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<PaginationResult<RoleModel>>(result);

            var role = result.Items.First();

            Assert.IsNotNull(role);
            Assert.AreEqual(roleId, role.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetRoleDetailsShouldReturnNotFoundForNonExistingGroup()
        {
            // Arrange
            var rolesController = CreateRolesController();
            var roleId = Guid.NewGuid().ToString();

            _ = this.mockRoleService
                .Setup(x => x.GetRoleDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((RoleDetailsModel)null);

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await rolesController.GetRoleDetails(roleId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateRoleShouldReturnOk()
        {
            // Arrange
            var rolesController = CreateRolesController();

            var role = new RoleDetailsModel()
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

            _ = this.mockRoleService
                .Setup(x => x.CreateRole(It.Is<RoleDetailsModel>(c => c.Id.Equals(role.Id, StringComparison.Ordinal))))
                .ReturnsAsync(role);

            // Act
            var result = await rolesController.CreateRoleAsync(role);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<RoleDetailsModel>(okObjectResult.Value);

            var roleObj = okObjectResult.Value as RoleDetailsModel;
            Assert.IsNotNull(roleObj);
            Assert.AreEqual(role.Id, roleObj.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateRoleShouldReturnBadRequestWhenCreationFails()
        {
            // Arrange
            var rolesController = CreateRolesController();
            var role = new RoleDetailsModel()
            {
                Id = Guid.NewGuid().ToString()
            };

            _ = this.mockRoleService
                .Setup(x => x.CreateRole(It.IsAny<RoleDetailsModel>()))
                .Throws(new Exception());

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await rolesController.CreateRoleAsync(role);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);

            this.mockRepository.VerifyAll();
        }


        [Test]
        public async Task UpdateRoleShouldReturnOkResult()
        {
            // Arrange
            var rolesController = CreateRolesController();


            var roleId = Guid.NewGuid().ToString();

            var role = new RoleDetailsModel()
            {
                Id = roleId
            };

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockRoleService
                .Setup(x => x.UpdateRole(roleId, It.Is<RoleDetailsModel>(c => c.Id.Equals(role.Id, StringComparison.Ordinal))))
                .ReturnsAsync(role);

            // Act
            var result = await rolesController.EditRoleAsync(roleId, role);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task EditRoleAsync_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var rolesController = CreateRolesController();
            var roleId = Guid.NewGuid().ToString();
            var role = new RoleDetailsModel() { Id = roleId };

            _ = this.mockRoleService
                .Setup(x => x.UpdateRole(roleId, It.IsAny<RoleDetailsModel>()))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await rolesController.EditRoleAsync(roleId, role));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteRoleShouldReturnExpectedBehavior()
        {
            // Arrange
            var rolesController = CreateRolesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockRoleService.Setup(c => c.DeleteRole(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(true);

            // Act
            var result = await rolesController.DeleteRole(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteRole_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var rolesController = CreateRolesController();
            var roleId = Guid.NewGuid().ToString();

            _ = this.mockRoleService.Setup(c => c.DeleteRole(It.Is<string>(x => x == roleId)))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await rolesController.DeleteRole(roleId));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }
    }
}
