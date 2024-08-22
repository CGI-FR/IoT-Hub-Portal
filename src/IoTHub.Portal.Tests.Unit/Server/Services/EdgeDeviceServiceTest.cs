// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Helpers;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
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
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IEdgeEnrollementHelper> mockEnrollementHelper;
        private Mock<ConfigHandler> mockConfig;

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
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockEnrollementHelper = MockRepository.Create<IEdgeEnrollementHelper>();
            this.mockConfig = MockRepository.Create<ConfigHandler>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfig.Object);
            _ = ServiceCollection.AddSingleton(this.mockEnrollementHelper.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IEdgeDevicesService, AzureEdgeDevicesService>();

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

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetPaginatedListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<Expression<Func<EdgeDevice, bool>>>(), default, new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels }))
                .ReturnsAsync(new PaginatedResult<EdgeDevice>
                {
                    Data = expectedEdgeDevices.Skip(expectedCurrentPage * expectedPageSize).Take(expectedPageSize).ToList(),
                    PageSize = expectedPageSize,
                    CurrentPage = expectedCurrentPage,
                    TotalCount = expectedTotalDevicesCount
                });

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

            var labelFilter = new List<string>()
            {
                "label01"
            };

            var deviceModelId = Fixture.Create<string>();
            var expectedEdgeDevices = new List<EdgeDevice>
            {
                new EdgeDevice
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
                    DeviceModelId = deviceModelId,
                    Scope = Fixture.Create<string>(),
                    ConnectionState = Fixture.Create<string>(),
                    NbDevices = 1,
                },
                new EdgeDevice
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
                    DeviceModelId = deviceModelId,
                    Scope = Fixture.Create<string>(),
                    ConnectionState = Fixture.Create<string>(),
                    NbDevices = 1
                }
            };

            var expectedTotalDevicesCount = 1;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;

            await DbContext.AddRangeAsync(expectedEdgeDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetPaginatedListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<Expression<Func<EdgeDevice, bool>>>(), default, new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels }))
                .ReturnsAsync(new PaginatedResult<EdgeDevice>
                {
                    Data = expectedEdgeDevices,
                    PageSize = expectedPageSize,
                    CurrentPage = expectedCurrentPage,
                    TotalCount = expectedTotalDevicesCount
                });

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(Fixture.Create<Uri>());

            // Act
            var result = await this.edgeDevicesService.GetEdgeDevicesPage(searchText: keywordFilter, searchStatus: true, modelId: deviceModelId, labels: labelFilter);

            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            _ = result.Data.First().DeviceId.Should().Be("test");
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
                .Setup(x => x.GetByIdAsync(It.Is<string>(c => c.Equals(expectedEdgeDevice.Id, StringComparison.Ordinal)), d => d.Tags, d => d.Labels))
                .ReturnsAsync(expectedEdgeDevice);

            _ = this.mockDeviceService
                .Setup(x => x.GetDeviceTwinWithModule(It.Is<string>(c => c.Equals(expectedEdgeDevice.Id, StringComparison.Ordinal))))
                .ReturnsAsync(new Twin(expectedEdgeDevice.Id));

            _ = this.mockDeviceService
                .Setup(x => x.RetrieveLastConfiguration(It.IsAny<IoTEdgeDevice>()))
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
                .Setup(x => x.GetByIdAsync(It.Is<string>(c => c.Equals(deviceId, StringComparison.Ordinal)), d => d.Tags, d => d.Labels))
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
                Tags = new Dictionary<string, string>()
                {
                    { "test", "test" }
                }
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
        public async Task CreateEdgeDeviceDbUpdateExceptionIsThrownMaxLengthExceededExceptionIsThrown()
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
                .ThrowsAsync(new MaxLengthExceededException());

            // Act
            var act = () => this.edgeDevicesService.CreateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<MaxLengthExceededException>();

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
                Labels = Fixture.CreateMany<LabelDto>(2).ToList()
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

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = new List<DeviceTagValue>
                    {
                        new()
                        {
                            Id = Fixture.Create<string>()
                        }
                    },
                    Labels = Fixture.CreateMany<Label>(1).ToList()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
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

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(value: null);

            // Act
            var act = () => this.edgeDevicesService.UpdateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeDeviceDbUpdateExceptionIsThrownNumericOverflowExceptionIsThrown()
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

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(edgeDevice.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = new List<DeviceTagValue>
                    {
                        new()
                        {
                            Id = Fixture.Create<string>()
                        }
                    },
                    Labels = new List<Label>()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new NumericOverflowException());

            // Act
            var act = () => this.edgeDevicesService.UpdateEdgeDevice(edgeDevice);

            // Assert
            _ = await act.Should().ThrowAsync<NumericOverflowException>();

            MockRepository.VerifyAll();
        }

        [Test]
        public void WhenEdgeDeviceIsNullUpdateEdgeDeviceShouldThrowArgumentNullException()
        {
            // 
            // Assert
            _ = Assert.ThrowsAsync<ArgumentNullException>(() => this.edgeDevicesService.UpdateEdgeDevice(null));
        }

        [Test]
        public async Task DeleteEdgeDeviceAsyncDeviceExistDeviceDeleted()
        {
            // Arrange
            var deviceDto = new IoTEdgeDevice()
            {
                DeviceId = Fixture.Create<string>()
            };

            _ = this.mockDeviceService.Setup(service => service.DeleteDevice(deviceDto.DeviceId))
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = Fixture.CreateMany<DeviceTagValue>(5).ToList(),
                    Labels = Fixture.CreateMany<Label>(5).ToList()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceId))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDevicesService.DeleteEdgeDeviceAsync(deviceDto.DeviceId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeDeviceDeviceNotExistNothingIsDone()
        {
            // Arrange
            var deviceDto = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>()
            };

            _ = this.mockDeviceService.Setup(service => service.DeleteDevice(deviceDto.DeviceId))
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(value: null);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.edgeDevicesService.DeleteEdgeDeviceAsync(deviceDto.DeviceId);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEdgeDeviceDbUpdateExceptionIsRaisedReferenceConstraintExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new IoTEdgeDevice
            {
                DeviceId = Fixture.Create<string>()
            };

            _ = this.mockDeviceService.Setup(service => service.DeleteDevice(deviceDto.DeviceId))
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new EdgeDevice
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockEdgeDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceId))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new ReferenceConstraintException());

            // Act
            var act = () => this.edgeDevicesService.DeleteEdgeDeviceAsync(deviceDto.DeviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ReferenceConstraintException>();
            MockRepository.VerifyAll();
        }

        [TestCase("RestartModule", /*lang=json,strict*/ "{\"id\":\"aaa\",\"schemaVersion\":\"1.0\"}")]
        public async Task ExecuteMethodRestartModuleShouldExecuteC2DMethod(string methodName, string expected)
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

        [TestCase("test")]
        public async Task ExecuteMethodTestShouldExecuteC2DMethod(string methodName)
        {
            // Arrange
            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "aaa",
            };

            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceService.Setup(c => c.ExecuteCustomCommandC2DMethod(
                It.Is<string>(x => x == deviceId),
                It.Is<string>(x => x.Equals(edgeModule.ModuleName, StringComparison.Ordinal)),
                It.Is<CloudToDeviceMethod>(x => x.MethodName == methodName)))
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

        [Test]
        public async Task GetAvailableLabels_LabelsExists_LabelsReturned()
        {
            // Arrange
            var edgeDevices = Fixture.CreateMany<EdgeDevice>(1);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetAllAsync(null, default, new Expression<Func<EdgeDevice, object>>[] { d => d.Labels, d => d.DeviceModel.Labels }))
                .ReturnsAsync(edgeDevices);

            var expectedLabels = Mapper.Map<List<LabelDto>>(edgeDevices.SelectMany(d => d.Labels)
                .Union(edgeDevices.SelectMany(d => d.DeviceModel.Labels)));

            // Act
            var result = await this.edgeDevicesService.GetAvailableLabels();

            // Assert
            _ = result.Count().Should().Be(expectedLabels.Count);
            MockRepository.VerifyAll();
        }
    }
}
