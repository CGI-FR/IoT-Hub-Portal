// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Services;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using Shared.Models.v1._0;
    using UnitTests.Bases;
    using Device = Portal.Domain.Entities.Device;

    [TestFixture]
    public class DeviceServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IExternalDeviceService> mockExternalDevicesService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceTwinMapper<DeviceListItem, DeviceDetails>> mockDeviceTwinMapper;

        private IDeviceService<DeviceDetails> deviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockExternalDevicesService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceTwinMapper = MockRepository.Create<IDeviceTwinMapper<DeviceListItem, DeviceDetails>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDevicesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IDeviceService<DeviceDetails>, DeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceService = Services.GetRequiredService<IDeviceService<DeviceDetails>>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetDevices_DefaultFilter_ReturnsExpectedDevices()
        {
            // Arrange
            var expectedTotalDevicesCount = 50;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;
            var expectedDevices = Fixture.CreateMany<Device>(expectedTotalDevicesCount/2).ToList();
            var expectedLorawanDevices = Fixture.CreateMany<LorawanDevice>(expectedTotalDevicesCount/2).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            await DbContext.AddRangeAsync(expectedLorawanDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(Fixture.Create<Uri>());

            // Act
            var result = await this.deviceService.GetDevices();

            // Assert
            _ = result.Data.Count.Should().Be(expectedPageSize);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevices_CustomFilter_ReturnsExpectedDevices()
        {
            // Arrange
            var keywordFilter = "WaNt tHiS DeViCe";

            var tagFilter = new Dictionary<string, string>
            {
                {"location", "FR"}
            };

            var labelFilter = new List<string>()
            {
                "label01"
            };

            var device1 = new Device
            {
                IsConnected = false,
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
                Labels = new List<Label>()
                {
                    new()
                    {
                        Name = "label01",
                        Color = "green"
                    }
                },
                DeviceModel = Fixture.Create<DeviceModel>()
            };

            var device2 = new Device
            {
                IsConnected = true,
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
                Labels = new List<Label>()
                {
                    new()
                    {
                        Name = "label01",
                        Color = "green"
                    }
                },
                DeviceModel = Fixture.Create<DeviceModel>()
            };

            var expectedTotalDevicesCount = 1;
            var expectedPageSize = 10;
            var expectedCurrentPage = 0;

            _ = await DbContext.AddAsync(device1);
            _ = await DbContext.AddAsync(device2);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockDeviceTagService.Setup(service => service.GetAllSearchableTagsNames())
                .Returns(new List<string> { "location" });

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(Fixture.Create<Uri>());

            // Act
            var result = await this.deviceService.GetDevices(searchText: keywordFilter, searchState: false, searchStatus: true, tags: tagFilter, labels: labelFilter);

            // Assert
            _ = result.Data.Count.Should().Be(expectedTotalDevicesCount);
            _ = result.TotalCount.Should().Be(expectedTotalDevicesCount);
            _ = result.PageSize.Should().Be(expectedPageSize);
            _ = result.CurrentPage.Should().Be(expectedCurrentPage);
            _ = result.Data.First().DeviceName.Should().Be(device1.Name);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevice_DeviceExist_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<Device>();

            var expectedImageUri = Fixture.Create<Uri>();
            var expectedDeviceDto = Mapper.Map<DeviceDetails>(expectedDevice);
            expectedDeviceDto.ImageUrl = expectedImageUri;

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(expectedDevice);

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockDeviceTagService.Setup(service => service.GetAllTagsNames())
                .Returns(expectedDevice.Tags.Select(tag => tag.Name));

            // Act
            var result = await this.deviceService.GetDevice(expectedDeviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevice_DeviceNotExist_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync((Device)null);

            // Act
            var act = () => this.deviceService.GetDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_NewDevice_DeviceCreated()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<DeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.deviceService.CreateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_DbUpdateExceptionIsThrown_InternalServerErrorExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<DeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new UniqueConstraintException());

            // Act
            var act = () => this.deviceService.CreateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<UniqueConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DeviceExist_DeviceUpdated()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<DeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new Device
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

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.deviceService.UpdateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DeviceNotExist_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<DeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((Device)null);

            // Act
            var act = () => this.deviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DbUpdateExceptionIsRaised_CannotInsertNullExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<DeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new Device
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new CannotInsertNullException());

            // Act
            var act = () => this.deviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<CannotInsertNullException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DeviceExist_DeviceDeleted()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new Device
                {
                    Tags = Fixture.CreateMany<DeviceTagValue>(5).ToList(),
                    Labels = Fixture.CreateMany<Label>(5).ToList()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DeviceNotExist_NothingIsDone()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((Device)null);

            // Act
            await this.deviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DbUpdateExceptionIsRaised_DbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new Device
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.deviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            _ = await act.Should().ThrowAsync<DbUpdateException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetCredentials_DeviceExist_ReturnsEnrollmentCredentials()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var expectedEnrollmentCredentials = Fixture.Create<DeviceCredentials>();

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceCredentials(deviceDto))
                .ReturnsAsync(expectedEnrollmentCredentials);

            // Act
            var result = await this.deviceService.GetCredentials(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedEnrollmentCredentials);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTelemetry_AnyDeviceId_ReturnsEmptyArray()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var expectedTelemetry = Array.Empty<LoRaDeviceTelemetryDto>();

            // Act
            var result = await this.deviceService.GetDeviceTelemetry(deviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTelemetry);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_AnyEventData_NothingIsDone()
        {
            // Arrange
            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(Fixture.Create<string>()));

            // Act
            await this.deviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CheckIfDeviceExistsShouldReturnFalseIfDeviceDoesNotExist()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync((Device)null);

            // Act
            var result = await this.deviceService.CheckIfDeviceExists(deviceId);

            // Assert
            Assert.IsFalse(result);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CheckIfDeviceExistsShouldReturnTrueIfDeviceExists()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync(new Device());

            // Act
            var result = await this.deviceService.CheckIfDeviceExists(deviceId);

            // Assert
            Assert.IsTrue(result);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAvailableLabels_LabelsExists_LabelsReturned()
        {
            // Arrange
            var devices = Fixture.CreateMany<Device>(1);
            var loraDevices = Fixture.CreateMany<LorawanDevice>(1);

            var expectedLabels = Mapper.Map<List<LabelDto>>(devices.SelectMany(d => d.Labels)
                .Union(devices.SelectMany(d => d.DeviceModel.Labels))
                .Union(loraDevices.SelectMany(d => d.Labels))
                .Union(loraDevices.SelectMany(d => d.DeviceModel.Labels)));

            await DbContext.AddRangeAsync(devices);
            await DbContext.AddRangeAsync(loraDevices);

            _ = await DbContext.SaveChangesAsync();

            // Act
            var result = await this.deviceService.GetAvailableLabels();

            // Assert
            _ = result.Count().Should().Be(expectedLabels.Count);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DeviceNotFoundOnAzure_DeviceDeletedInDatabase()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .ThrowsAsync(new DeviceNotFoundException(deviceDto.DeviceID));

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new Device
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.deviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
