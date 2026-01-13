// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using IoTHub.Portal.Server.Controllers.v10;

    [TestFixture]
    public class AccessControlssControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<AccessControlController>> mockLogger;
        private Mock<IAccessControlManagementService> mockAccessControlService;
        private Mock<IUrlHelper> mockUrlHelper;
        private IDataProtectionProvider mockDataProtectionProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<AccessControlController>>();
            this.mockAccessControlService = this.mockRepository.Create<IAccessControlManagementService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDataProtectionProvider = new EphemeralDataProtectionProvider();
        }

        private AccessControlController CreateAccessControlsController()
        {
            return new AccessControlController(
                this.mockLogger.Object,
                this.mockAccessControlService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllAccessControlsReturnOkResult()
        {
            // Arrange
            var accessControlController = CreateAccessControlsController();

            var paginedAccessControls = new PaginatedResult<AccessControlModel>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new AccessControlModel()
                {
                    Id = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockAccessControlService
                .Setup(x => x.GetAccessControlPage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(paginedAccessControls);

            var locationUrl = "http://location/accessControls";

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
            var result = await accessControlController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(paginedAccessControls.Data.Count, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Get_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var accessControlController = CreateAccessControlsController();

            _ = this.mockAccessControlService
                .Setup(x => x.GetAccessControlPage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<string>()
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
        public async Task GetByIdShouldReturnTheCorrespondantAccessControl()
        {
            // Arrange
            var accessControlsController = CreateAccessControlsController();

            var accessControlId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.IsAny<It.IsAnyType>(),
                 It.IsAny<Exception>(),
                 (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
             ));
            _ = this.mockAccessControlService
                .Setup(x => x.GetAccessControlPage(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new PaginatedResult<AccessControlModel>()
                {
                    Data = new List<AccessControlModel>() { new AccessControlModel() { Id = accessControlId } },
                    TotalCount = 1,
                    PageSize = 10,
                    CurrentPage = 0
                });

            // Act
            var result = await accessControlsController.Get(accessControlId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<PaginationResult<AccessControlModel>>(result);

            var accessControl = result.Items.First();

            Assert.IsNotNull(accessControl);
            Assert.AreEqual(accessControlId, accessControl.Id);

            this.mockRepository.VerifyAll();
        }


        [Test]
        public async Task GetAccessControlDetailsShouldReturnNotFoundForNonExistingGroup()
        {
            // Arrange
            var accessControlController = CreateAccessControlsController();
            var accessControlId = Guid.NewGuid().ToString();

            _ = this.mockAccessControlService
                .Setup(x => x.GetAccessControlAsync(It.IsAny<string>()))
                .ReturnsAsync((AccessControlModel)null);

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await accessControlController.GetACById(accessControlId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateAccessControlShouldReturnOk()
        {
            // Arrange
            var accessControlsController = CreateAccessControlsController();

            var accessControl = new AccessControlModel()
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

            _ = this.mockAccessControlService
                .Setup(x => x.CreateAccessControl(It.Is<AccessControlModel>(c => c.Id.Equals(accessControl.Id, StringComparison.Ordinal))))
                .ReturnsAsync(accessControl);

            // Act
            var result = await accessControlsController.CreateAccessControlAsync(accessControl);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<AccessControlModel>(okObjectResult.Value);

            var accessControlObj = okObjectResult.Value as AccessControlModel;
            Assert.IsNotNull(accessControlObj);
            Assert.AreEqual(accessControl.Id, accessControlObj.Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateAccessControlShouldReturnBadRequestWhenCreationFails()
        {
            // Arrange
            var accessControlController = CreateAccessControlsController();
            var accessControl = new AccessControlModel()
            {
                Id = Guid.NewGuid().ToString()
            };

            _ = this.mockAccessControlService
                .Setup(x => x.CreateAccessControl(It.IsAny<AccessControlModel>()))
                .Throws(new Exception());

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var result = await accessControlController.CreateAccessControlAsync(accessControl);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateAccessControlShouldReturnOkResult()
        {
            // Arrange
            var accessControlsController = CreateAccessControlsController();


            var accessControlId = Guid.NewGuid().ToString();

            var accessControl = new AccessControlModel()
            {
                Id = accessControlId
            };

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockAccessControlService
                .Setup(x => x.UpdateAccessControl(accessControlId, It.Is<AccessControlModel>(c => c.Id.Equals(accessControl.Id, StringComparison.Ordinal))))
                .ReturnsAsync(accessControl);

            // Act
            var result = await accessControlsController.EditAccessControlAsync(accessControlId, accessControl);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task EditAccessControlAsync_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var acController = CreateAccessControlsController();
            var acId = Guid.NewGuid().ToString();
            var ac = new AccessControlModel() { Id = acId };

            _ = this.mockAccessControlService
                .Setup(x => x.UpdateAccessControl(acId, It.IsAny<AccessControlModel>()))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await acController.EditAccessControlAsync(acId, ac));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAccessControlShouldReturnExpectedBehavior()
        {
            // Arrange
            var accessControlsController = CreateAccessControlsController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            _ = this.mockAccessControlService.Setup(c => c.DeleteAccessControl(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(true);

            // Act
            var result = await accessControlsController.DeleteAccessControl(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAccessControl_ShouldReturnInternalServerErrorOnException()
        {
            // Arrange
            var acController = CreateAccessControlsController();
            var acId = Guid.NewGuid().ToString();

            _ = this.mockAccessControlService.Setup(c => c.DeleteAccessControl(It.Is<string>(x => x == acId)))
                .ThrowsAsync(new Exception("Test exception"));

            _ = this.mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ));

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await acController.DeleteAccessControl(acId));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual("Test exception", ex.Message);

            this.mockRepository.VerifyAll();
        }
    }
}
