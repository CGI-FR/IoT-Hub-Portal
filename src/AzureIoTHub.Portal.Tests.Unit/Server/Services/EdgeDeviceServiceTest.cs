// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceServiceTest
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;
        private Mock<IEdgeDeviceMapper> mockEdgeDeviceMapper;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceProvisioningServiceManager> mockProvisioningServiceManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockEdgeDeviceMapper = this.mockRepository.Create<IEdgeDeviceMapper>();
        }

        private EdgeDevicesService CreateEdgeDeviceService()
        {
            return new EdgeDevicesService(
                this.mockRegistryManager.Object,
                this.mockDeviceService.Object,
                this.mockEdgeDeviceMapper.Object,
                this.mockProvisioningServiceManager.Object,
                this.mockDeviceTagService.Object);
        }

        [Test]
        public void GetEdgeDevicesPageShouldReturnList()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            const int count = 100;
            var paginationResultSimul = new PaginationResult<Twin>
            {
                Items = Enumerable.Range(0, 100).Select(x => new Twin
                {
                    DeviceId = FormattableString.Invariant($"{x}"),
                }),
                TotalItems = 1000,
                NextPage = Guid.NewGuid().ToString()

            };

            _ = this.mockEdgeDeviceMapper
                .Setup(x => x.CreateEdgeDeviceListItem(It.IsAny<Twin>()))
                .Returns(new IoTEdgeListItem());

            var mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            _ = mockUrlHelper.Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(Guid.NewGuid().ToString());

            // Act
            var result = edgeDeviceService.GetEdgeDevicesPage(paginationResultSimul,
                mockUrlHelper.Object,
                searchText: "bbb",
                searchStatus: true,
                pageSize: 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Items.Count());
            Assert.AreEqual(1000, result.TotalItems);
            Assert.IsNotNull(result.NextPage);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceShouldReturnValue()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var expectedDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Status = DeviceStatus.Enabled.ToString(),
                NbDevices = 2,
                NbModules = 1,
                LastDeployment = new ConfigItem(),
                Modules = new List<IoTEdgeModule>()
                {
                    new IoTEdgeModule()
                }
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(expectedDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Twin(expectedDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwinWithModule(It.Is<string>(c => c.Equals(expectedDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Twin(expectedDevice.DeviceId));

            var edgeHubTwin = new Twin("edgeHub");
            edgeHubTwin.Properties.Reported["clients"] = new[]
            {
                1, 2
            };

            var mockQuery = this.mockRepository.Create<IQuery>();
            _ = mockQuery.Setup(c => c.GetNextAsTwinAsync())
                .ReturnsAsync(new[] { edgeHubTwin });

            _ = this.mockRegistryManager.Setup(c => c.CreateQuery(
                It.Is<string>(x => x == $"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{expectedDevice.DeviceId}']"),
                It.Is<int>(x => x == 1)))
                .Returns(mockQuery.Object);

            _ = this.mockEdgeDeviceMapper
                .Setup(x => x.CreateEdgeDevice(It.Is<Twin>(c => c.DeviceId.Equals(expectedDevice.DeviceId, StringComparison.Ordinal)),
                It.Is<Twin>(c => c.DeviceId.Equals(expectedDevice.DeviceId, StringComparison.Ordinal)),
                It.Is<int>(c => c.Equals(2)), It.IsAny<ConfigItem>(), It.IsAny<IEnumerable<string>>()))
                .Returns(expectedDevice);

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames()).Returns(new List<string>());

            // Act
            var result = await edgeDeviceService.GetEdgeDevice(expectedDevice.DeviceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDevice, result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceTwinIsNullGetEdgeDeviceShouldResourceNotFoundException()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal))))
                .ReturnsAsync(value: null);

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => edgeDeviceService.GetEdgeDevice(deviceId));

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeDeviceShouldReturnvalue()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var mockResult = new BulkRegistryOperationResult
            {
                IsSuccessful = true
            };

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = "aaa",
            };

            _ = this.mockDeviceService.Setup(c => c.CreateDeviceWithTwin(
                It.Is<string>(x => x == edgeDevice.DeviceId),
                It.Is<bool>(x => x),
                It.Is<Twin>(x => x.DeviceId == edgeDevice.DeviceId),
                It.Is<DeviceStatus>(x => x == DeviceStatus.Enabled)))
                .ReturnsAsync(mockResult);

            // Act
            var result = await edgeDeviceService.CreateEdgeDevice(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockResult.IsSuccessful, result.IsSuccessful);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeDeviceIsNullCreateEdgeDeviceShouldThrowArgumentNullException()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            // Act

            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => edgeDeviceService.CreateEdgeDevice(null));
        }

        [Test]
        public async Task UpdateEdgeDeviceShouldReturnUpdatedTwin()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Status = DeviceStatus.Enabled.ToString(),
            };

            var mockTwin = new Twin(edgeDevice.DeviceId);
            mockTwin.Tags["env"] = "dev";

            _ = this.mockDeviceService
                .Setup(x => x.GetDevice(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDevice(It.Is<Device>(c => c.Id.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDeviceTwin(It.Is<Twin>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            // Act
            var result = await edgeDeviceService.UpdateEdgeDevice(edgeDevice);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeDeviceIsNullUpdateEdgeDeviceShouldThrowArgumentNullException()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => edgeDeviceService.UpdateEdgeDevice(null));
        }

        [TestCase("RestartModule", /*lang=json,strict*/ "{\"id\":\"aaa\",\"schemaVersion\":\"1.0\"}")]
        public async Task ExecuteMethodShouldExecuteC2DMethod(string methodName, string expected)
        {
            // Arrange
            var edgeDeviceService  = CreateEdgeDeviceService();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.ExecuteC2DMethod(
                It.Is<string>(x => x == deviceId),
                It.Is<CloudToDeviceMethod>(x =>
                    x.MethodName == methodName
                    && x.GetPayloadAsJson() == expected
                )))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            // Act
            _ = await edgeDeviceService.ExecuteModuleMethod(deviceId, edgeModule.ModuleName, methodName);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [TestCase("RestartModule")]
        public void WhenEdgeModuleIsNullExecuteMethodShouldThrowArgumentNullException(string methodName)
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var deviceId = Guid.NewGuid().ToString();

            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => edgeDeviceService.ExecuteModuleMethod(deviceId, null, methodName));
        }

        [TestCase("test")]
        public async Task ExecuteModuleCommand(string commandName)
        {
            // Arrange
            var edgeDeviceService  = CreateEdgeDeviceService();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.ExecuteCustomCommandC2DMethod(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x == edgeModule.ModuleName),
                It.Is<CloudToDeviceMethod>(x =>
                    x.MethodName == commandName
                )))
                .ReturnsAsync(new CloudToDeviceMethodResult
                {
                    Status = 200
                });

            // Act
            var result = await edgeDeviceService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, commandName);

            // Assert
            Assert.AreEqual(200, result.Status);
            this.mockRepository.VerifyAll();
        }

        [TestCase("test")]
        public void WhenDeviceIdIsNullExecuteModuleCommandShouldThrowArgumentNullException(string commandName)
        {
            // Arrange
            var edgeDeviceService  = CreateEdgeDeviceService();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = string.Empty;

            // Act
            var result = async () => await edgeDeviceService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, commandName);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [TestCase("test")]
        public void WhenModuleIsNullExecuteModuleCommandShouldThrowArgumentNullException(string commandName)
        {
            // Arrange
            var edgeDeviceService  = CreateEdgeDeviceService();

            var deviceId = Guid.NewGuid().ToString();

            // Act
            var result = async () => await edgeDeviceService.ExecuteModuleCommand(deviceId, null, commandName);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenCommandNameIsNullExecuteModuleCommandShouldThrowArgumentNullException()
        {
            // Arrange
            var edgeDeviceService  = CreateEdgeDeviceService();

            var deviceId = string.Empty;

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            // Act
            var result = async () => await edgeDeviceService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, null);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceCredentialsShouldReturnEnrollmentCredentials()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            var mockRegistrationCredentials = new EnrollmentCredentials
            {
                RegistrationID = "aaa",
                SymmetricKey = "dfhjkfdgh"
            };

            var mockTwin = new Twin("aaa");

            _ = this.mockProvisioningServiceManager.Setup(c => c.GetEnrollmentCredentialsAsync("aaa", It.IsAny<string>()))
                .ReturnsAsync(mockRegistrationCredentials);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(mockTwin);

            // Act
            var result = await edgeDeviceService.GetEdgeDeviceCredentials("aaa");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockRegistrationCredentials, result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceTwinIsNullGetEdgeDeviceCredentialsShouldThrowResourceNotFoundException()
        {
            // Arrange
            var edgeDeviceService = CreateEdgeDeviceService();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(value: null);

            // Act

            // Assert
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => edgeDeviceService.GetEdgeDeviceCredentials("aaa"));

            this.mockRepository.VerifyAll();
        }
    }
}
