// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceServiceTest : BackendUnitTest
    {
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDeviceRepository> mockEdgeDeviceRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        private IEdgeDevicesService edgeDevicesService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeDeviceRepository = MockRepository.Create<IEdgeDeviceRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IEdgeDevicesService, EdgeDevicesService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.edgeDevicesService = Services.GetRequiredService<IEdgeDevicesService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetEdgeDevicesPageShouldReturnList()
        {
            // Arrange
            var expectedTotalDevicesCount = 50;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;
            var expectedEdgeDevices = Fixture.CreateMany<EdgeDevice>(expectedTotalDevicesCount).ToList();

            await DbContext.AddRangeAsync(expectedEdgeDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(Fixture.Create<Uri>());

            // Act
            var result = await this.edgeDevicesService.GetEdgeDevicesPage();

            // Assert
            Assert.IsNotNull(result);
            _ = result.Data.Count.Should().Be(expectedPageSize);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDevicesCustomFilterReturnsExpectedDevices()
        {
            // Arrange
            var keywordFilter = "WaNt tHiS DeViCe";

            var device1 = new EdgeDevice
            {
                Id = "test",
                IsEnabled = true,
                Name = "I want this device",
                Tags = new List<DeviceTagValue>
                {
                    new()
                    {
                        Name = "location",
                        Value = "FR"
                    }
                },
                DeviceModelId = Fixture.Create<string>(),
                Scope = Fixture.Create<string>(),
                NbDevices = 1,
            };

            var device2 = new EdgeDevice
            {
                IsEnabled = false,
                Name = "I don't want this device",
                Tags = new List<DeviceTagValue>
                {
                    new()
                    {
                        Name = "location",
                        Value = "US"
                    }
                },
                DeviceModelId = Fixture.Create<string>(),
                Scope = Fixture.Create<string>(),
                NbDevices = 1
            };

            var expectedTotalDevicesCount = 1;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;

            _ = await DbContext.AddAsync(device1);
            _ = await DbContext.AddAsync(device2);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(Fixture.Create<Uri>());

            // Act
            var result = await this.edgeDevicesService.GetEdgeDevicesPage(searchText: keywordFilter, searchStatus: true);

            _ = result.Data.Count.Should().Be(expectedTotalDevicesCount);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            _ = result.Data.First().DeviceId.Should().Be(device1.Id);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeDeviceShouldReturnValue()
        {
            // Arrange
            var expectedEdgeDevice = Fixture.Create<EdgeDevice>();

            var expectedImageUri = Fixture.Create<Uri>();
            var expectedEdgeDeviceDto = Mapper.Map<IoTEdgeDevice>(expectedEdgeDevice);
            expectedEdgeDeviceDto.ImageUrl = expectedImageUri;

            _ = this.mockEdgeDeviceRepository
                .Setup(x => x.GetByIdAsync(It.Is<string>(c => c.Equals(expectedEdgeDevice.Id, StringComparison.Ordinal))))
                .ReturnsAsync(expectedEdgeDevice);

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwinWithModule(It.Is<string>(c => c.Equals(expectedEdgeDevice.Id, StringComparison.Ordinal))))
                .ReturnsAsync(new Twin(expectedEdgeDevice.Id));

            _ = this.mockDeviceService
                .Setup(x => x.RetrieveLastConfiguration(It.IsAny<Twin>()))
                .ReturnsAsync(new ConfigItem());

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockDeviceTagService.Setup(service => service.GetAllTagsNames())
                .Returns(expectedEdgeDevice.Tags.Select(tag => tag.Name));


            _ = this.mockDeviceTagService.Setup(x => x.GetAllTagsNames()).Returns(new List<string>());

            // Act
            var result = await this.edgeDevicesService.GetEdgeDevice(expectedEdgeDevice.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeDeviceDto.DeviceId, result.DeviceId);
            Assert.AreEqual(expectedEdgeDeviceDto.DeviceName, result.DeviceName);

            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceTwinIsNullGetEdgeDeviceShouldResourceNotFoundException()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceRepository
                .Setup(x => x.GetByIdAsync(It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal))))
                .ReturnsAsync(value: null);

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(() => this.edgeDevicesService.GetEdgeDevice(deviceId));

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeDeviceShouldReturnvalue()
        {
            // Arrange
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

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.edgeDevicesService.CreateEdgeDevice(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(edgeDevice.DeviceId, result.DeviceId);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateEdgeDeviceDbUpdateExceptionIsThrownInternalServerErrorExceptionIsThrown()
        {
            // Arrange
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

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.edgeDevicesService.CreateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeDeviceIsNullCreateEdgeDeviceShouldThrowArgumentNullException()
        {
            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.edgeDevicesService.CreateEdgeDevice(null));
        }

        [Test]
        public async Task UpdateEdgeDeviceShouldReturnUpdatedTwin()
        {
            // Arrange
            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Status = DeviceStatus.Enabled.ToString(),
            };

            var mockTwin = new Twin(edgeDevice.DeviceId);
            mockTwin.Tags["deviceName"] = "Test";

            _ = this.mockDeviceService
                .Setup(x => x.GetDevice(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDevice(It.Is<Microsoft.Azure.Devices.Device>(c => c.Id.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDeviceTwin(It.Is<Twin>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = new List<DeviceTagValue>
                    {
                        new()
                        {
                            Id = Fixture.Create<string>()
                        }
                    }
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.edgeDevicesService.UpdateEdgeDevice(edgeDevice);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(edgeDevice.DeviceId, result.DeviceId);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeDeviceDeviceNotExistResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Status = DeviceStatus.Enabled.ToString(),
            };

            var mockTwin = new Twin(edgeDevice.DeviceId);
            mockTwin.Tags["deviceName"] = "Test";

            _ = this.mockDeviceService
                .Setup(x => x.GetDevice(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDevice(It.Is<Microsoft.Azure.Devices.Device>(c => c.Id.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDeviceTwin(It.Is<Twin>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId))
                .ReturnsAsync(value: null);

            // Act
            var act = () => this.edgeDevicesService.UpdateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeDeviceDbUpdateExceptionIsThrownInternalServerErrorExceptionIsThrown()
        {
            // Arrange
            var edgeDevice = new IoTEdgeDevice()
            {
                DeviceId = Guid.NewGuid().ToString(),
                Status = DeviceStatus.Enabled.ToString(),
            };

            var mockTwin = new Twin(edgeDevice.DeviceId);
            mockTwin.Tags["deviceName"] = "Test";

            _ = this.mockDeviceService
                .Setup(x => x.GetDevice(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDevice(It.Is<Microsoft.Azure.Devices.Device>(c => c.Id.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device(edgeDevice.DeviceId));

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwin(It.Is<string>(c => c.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockDeviceService
                .Setup(x => x.UpdateDeviceTwin(It.Is<Twin>(c => c.DeviceId.Equals(edgeDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(mockTwin);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = new List<DeviceTagValue>
                    {
                        new()
                        {
                            Id = Fixture.Create<string>()
                        }
                    }
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.edgeDevicesService.UpdateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeDeviceIsNullUpdateEdgeDeviceShouldThrowArgumentNullException()
        {
            // 
            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.edgeDevicesService.UpdateEdgeDevice(null));
        }

        [TestCase("RestartModule", /*lang=json,strict*/ "{\"id\":\"aaa\",\"schemaVersion\":\"1.0\"}")]
        public async Task ExecuteMethodShouldExecuteC2DMethod(string methodName, string expected)
        {
            // Arrange
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
            _ = await this.edgeDevicesService.ExecuteModuleMethod(deviceId, edgeModule.ModuleName, methodName);

            // Assert
            MockRepository.VerifyAll();
        }

        [TestCase("RestartModule")]
        public void WhenEdgeModuleIsNullExecuteMethodShouldThrowArgumentNullException(string methodName)
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.edgeDevicesService.ExecuteModuleMethod(deviceId, null, methodName));
        }

        [TestCase("test")]
        public async Task ExecuteModuleCommand(string commandName)
        {
            // Arrange
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
            var result = await this.edgeDevicesService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, commandName);

            // Assert
            Assert.AreEqual(200, result.Status);
            MockRepository.VerifyAll();
        }

        [TestCase("test")]
        public void WhenDeviceIdIsNullExecuteModuleCommandShouldThrowArgumentNullException(string commandName)
        {
            // Arrange
            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = string.Empty;

            // Act
            var result = async () => await this.edgeDevicesService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, commandName);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            MockRepository.VerifyAll();
        }

        [TestCase("test")]
        public void WhenModuleIsNullExecuteModuleCommandShouldThrowArgumentNullException(string commandName)
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            // Act
            var result = async () => await this.edgeDevicesService.ExecuteModuleCommand(deviceId, null, commandName);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenCommandNameIsNullExecuteModuleCommandShouldThrowArgumentNullException()
        {
            // Arrange
            var deviceId = string.Empty;

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            // Act
            var result = async () => await this.edgeDevicesService.ExecuteModuleCommand(deviceId, edgeModule.ModuleName, null);

            // Assert
            _ = result.Should().ThrowAsync<ArgumentNullException>();
            MockRepository.VerifyAll();
        }

    }
}
