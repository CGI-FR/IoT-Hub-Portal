// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.Xml;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Controllers.V10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph.DeviceManagement.DeviceConfigurations.Item.GetOmaSettingPlainTextValueWithSecretReferenceValueId;
    using Models.v10;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDevicesControllerTests : BackendUnitTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<EdgeDevicesController>> mockLogger;
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IEdgeDevicesService> mockEdgeDeviceService;
        private Mock<IUrlHelper> mockUrlHelper;
        private IDataProtectionProvider mockDataProtectionProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<EdgeDevicesController>>();
            this.mockDeviceService = this.mockRepository.Create<IExternalDeviceService>();
            this.mockEdgeDeviceService = this.mockRepository.Create<IEdgeDevicesService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockDataProtectionProvider = new EphemeralDataProtectionProvider();
        }

        private EdgeDevicesController CreateEdgeDevicesController(IDataProtectionProvider dataProtectionProvider = null)
        {
            return new EdgeDevicesController(
                this.mockLogger.Object,
                this.mockDeviceService.Object,
                this.mockEdgeDeviceService.Object,
                dataProtectionProvider ?? this.mockDataProtectionProvider)
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [Test]
        public async Task GetAllDeviceShouldReturnOkResult()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var expectedPaginedEdgeDevice = new PaginatedResult<IoTEdgeListItem>()
            {
                Data = Enumerable.Range(0, 10).Select(x => new IoTEdgeListItem()
                {
                    DeviceId = FormattableString.Invariant($"{x}"),
                }).ToList(),
                TotalCount = 100,
                PageSize = 10,
                CurrentPage = 0
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevicesPage(
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string[]>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync(expectedPaginedEdgeDevice);

            var locationUrl = "http://location/edge/devices";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var result = await edgeDeviceController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPaginedEdgeDevice.Data.Count, result.Items.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenSpecifyingIdGetShouldReturnTheEdgeDevice()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevice(It.IsAny<string>()))
                .ReturnsAsync(new IoTEdgeDevice()
                {
                    DeviceId = deviceId,
                });

            // Act
            var result = await edgeDeviceController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsAssignableFrom<IoTEdgeDevice>(okObjectResult.Value);

            var iotEdge = okObjectResult.Value as IoTEdgeDevice;

            Assert.IsNotNull(iotEdge);
            Assert.AreEqual(deviceId, iotEdge.DeviceId);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesNotExistGetEdgeDeviceSpecifyingIdShouldThrowAnException()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.GetEdgeDevice(It.IsAny<string>()))
                .ThrowsAsync(new DeviceNotFoundException(""));

            // Act
            var result = await edgeDeviceController.Get(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeDeviceAsyncShouldReturnOk()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.CreateEdgeDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(edgeDevice);

            // Act
            var result = await edgeDeviceController.CreateEdgeDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<IoTEdgeDevice>(okObjectResult.Value);

            var edgeDeviceObject = okObjectResult.Value as IoTEdgeDevice;
            Assert.IsNotNull(edgeDeviceObject);
            Assert.AreEqual(edgeDevice.DeviceId, edgeDeviceObject.DeviceId);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceAsyncShouldReturnOkResult()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
            };

            _ = this.mockEdgeDeviceService
                .Setup(x => x.UpdateEdgeDevice(It.Is<IoTEdgeDevice>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(edgeDevice);

            // Act
            var result = await edgeDeviceController.UpdateDeviceAsync(edgeDevice);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService.Setup(c => c.DeleteEdgeDeviceAsync(It.Is<string>(x => x == deviceId)))
                .Returns(Task.CompletedTask);

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            var result = await edgeDevicesController.DeleteDeviceAsync(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceLogsMustReturnLogsWhenNoErrorIsReturned()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(c => c.GetEdgeDeviceLogs(
                It.Is<string>(x => x == deviceId),
                It.Is<IoTEdgeModule>(x => x == edgeModule)))
                .ReturnsAsync(new List<IoTEdgeDeviceLog>
                {
                    new IoTEdgeDeviceLog
                    {
                        Id = deviceId,
                        Text = Guid.NewGuid().ToString(),
                        LogLevel = 1,
                        TimeStamp = DateTime.UtcNow
                    }
                });

            // Act
            var result = await edgeDevicesController.GetEdgeDeviceLogs(deviceId, edgeModule);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.Count().Should().Be(1);
        }

        [Test]
        public async Task GetEdgeDeviceLogsThrowsArgumentNullExceptionWhenModelIsNull()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();
            var deviceId = Guid.NewGuid().ToString();

            // Act
            var act = () => edgeDevicesController.GetEdgeDeviceLogs(deviceId, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task GetEnrollmentCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();

            var mockDevice = Fixture.Create<IoTEdgeDevice>();

            _ = this.mockEdgeDeviceService.Setup(x => x.GetEdgeDevice(mockDevice.DeviceId))
                .ReturnsAsync(mockDevice);

            _ = this.mockDeviceService
                .Setup(x => x.GetEdgeDeviceCredentials(mockDevice))
                .ReturnsAsync(new DeviceCredentials());

            // Act
            var result = await edgeDevicesController.GetCredentials(mockDevice.DeviceId);

            // Assert
            Assert.IsNotNull(result);

            var okObjectResult = result.Result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<DeviceCredentials>(okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesNotExistGetEnrollmentCredentialsShouldReturnNotFound()
        {
            // Arrange
            var edgeDevicesController = CreateEdgeDevicesController();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService.Setup(c => c.GetEdgeDevice(deviceId))
                .ReturnsAsync((IoTEdgeDevice)null);

            // Act
            var result = await edgeDevicesController.GetCredentials(deviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);

            this.mockRepository.VerifyAll();
        }

        [TestCase("RestartModule")]
        public async Task ExecuteMethodShouldExecuteC2DMethod(string methodName)
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var module = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.ExecuteModuleMethod(It.IsAny<string>(),
                It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal)),
                It.Is<string>(c => c.Equals(methodName, StringComparison.Ordinal))))
                .ReturnsAsync(new C2Dresult());

            // Act
            var result = await edgeDeviceController.ExecuteModuleMethod(module.ModuleName, deviceId, methodName);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [TestCase("test")]
        public async Task ExecuteCustomModuleMethodShouldExecuteC2DMethod(string commandName)
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var deviceId =  Guid.NewGuid().ToString();
            var moduleName = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceService
                .Setup(x => x.ExecuteModuleMethod(
                    It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal)),
                    It.Is<string>(c => c.Equals(moduleName, StringComparison.Ordinal)),
                    It.Is<string>(c => c.Equals(commandName, StringComparison.Ordinal))))
                .ReturnsAsync(new C2Dresult());

            // Act
            var result = await edgeDeviceController.ExecuteModuleMethod(deviceId, moduleName, commandName);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvailableLabels_ExistingLabels_ReturnsLabels()
        {
            // Arrange
            var edgeDeviceController = CreateEdgeDevicesController();

            var expectedLabels = new List<LabelDto>()
            {
                new LabelDto()
            };

            _ = this.mockEdgeDeviceService.Setup(service => service.GetAvailableLabels())
                .ReturnsAsync(expectedLabels);

            // Act
            var result = await edgeDeviceController.GetAvailableLabels();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLabels);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEnrollmentScriptUrlShouldReturnEncryptedUrlParameters()
        {
            // Arrange
            var encryptedData = Fixture.CreateMany<byte>(64).ToArray();
            var encodedPayload = WebEncoders.Base64UrlEncode(encryptedData);

            var fakeUri = Fixture.Create<Uri>();

            var deviceId = Fixture.Create<string>();
            var templateName = Fixture.Create<string>();

            var mockDataProtectionProvider = this.mockRepository.Create<IDataProtectionProvider>();
            var mockDataProtector = this.mockRepository.Create<IDataProtector>();

            _ = mockDataProtectionProvider.Setup(c => c.CreateProtector(It.IsAny<string>()))
                .Returns(mockDataProtector.Object);

            var edgeDeviceController = CreateEdgeDevicesController(mockDataProtectionProvider.Object);

            var mockLimitedLifetimeProtector = this.mockRepository
                                                    .Create<ITimeLimitedDataProtector>();

            _ = mockDataProtector.Setup(c => c.CreateProtector(It.IsAny<string>()))
                .Returns(mockLimitedLifetimeProtector.Object);

            _ = mockLimitedLifetimeProtector.Setup(c => c.Protect(It.IsAny<byte[]>()))
                .Returns(encryptedData);

            _ = this.mockUrlHelper.SetupGet(c => c.ActionContext)
                .Returns(new ActionContext
                {
                    HttpContext = new DefaultHttpContext()
                });

            _ = this.mockUrlHelper.Setup(c => c.Action(It.Is<UrlActionContext>(a => a.Values.ToString() == new { code = encodedPayload }.ToString())))
                .Returns(fakeUri.ToString());

            // Act
            var response = edgeDeviceController.GetEnrollementScriptUrl(deviceId, templateName);

            // Assert
            _ = response.Result.Should().BeAssignableTo<OkObjectResult>();
            _ = (response.Result as OkObjectResult).Value.Should().Be(fakeUri.ToString());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEnrollementScriptShouldDecryptTheCode()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var templateName = Fixture.Create<string>();

            var encryptedData = "{\"deviceId\": \"" + deviceId + "\",\"templateName\": \"" + templateName + "\"}";
            var expectedResumt = Fixture.Create<string>();

            var protector = this.mockDataProtectionProvider.CreateProtector(EdgeDevicesController.EdgeEnrollementKeyProtectorName)
                                                                .ToTimeLimitedDataProtector();

            var code = protector.Protect(encryptedData, DateTimeOffset.UtcNow + 1.Minutes());

            _ = this.mockEdgeDeviceService.Setup(c => c.GetEdgeDeviceEnrollementScript(deviceId, templateName))
                .ReturnsAsync(expectedResumt);

            var edgeDeviceController = CreateEdgeDevicesController();

            // Act
            var response = await edgeDeviceController.GetEnrollementScript(code);

            // Assert
            _ = response.Result.Should().BeAssignableTo<OkObjectResult>();
            _ = (response.Result as OkObjectResult).Value.Should().Be(expectedResumt);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenCodeExpiresGetEnrollementScriptShouldReturnBadRequest()
        {
            // Arrange
            var encryptedData = Fixture.Create<string>();

            var protector = this.mockDataProtectionProvider.CreateProtector(EdgeDevicesController.EdgeEnrollementKeyProtectorName)
                                                                .ToTimeLimitedDataProtector();

            var code = protector.Protect(encryptedData, DateTimeOffset.UtcNow - 1.Minutes());

            var edgeDeviceController = CreateEdgeDevicesController();

            // Act
            var response = await edgeDeviceController.GetEnrollementScript(code);

            // Assert
            _ = response.Result.Should().BeAssignableTo<BadRequestObjectResult>();
            this.mockRepository.VerifyAll();
        }
    }
}
