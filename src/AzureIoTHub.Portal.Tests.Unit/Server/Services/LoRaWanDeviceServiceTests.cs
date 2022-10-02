// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using UnitTests.Bases;
    using NUnit.Framework;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using FluentAssertions;
    using Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.Devices;
    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class LoRaWanDeviceServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<ILorawanDeviceRepository> mockLorawanDeviceRepository;
        private Mock<IExternalDeviceService> mockExternalDevicesService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;

        private IDeviceService<LoRaDeviceDetails> lorawanDeviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockLorawanDeviceRepository = MockRepository.Create<ILorawanDeviceRepository>();
            this.mockExternalDevicesService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceTwinMapper = MockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockLorawanDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDevicesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IDeviceService<LoRaDeviceDetails>, LoRaWanDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.lorawanDeviceService = Services.GetRequiredService<IDeviceService<LoRaDeviceDetails>>();
        }

        [Test]
        public async Task GetDevice_ExistingDevice_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<LorawanDevice>();

            var expectedImageUri = Fixture.Create<Uri>();
            var expectedDeviceDto = Mapper.Map<LoRaDeviceDetails>(expectedDevice);
            expectedDeviceDto.ImageUrl = expectedImageUri;

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceDto.DeviceID))
                .ReturnsAsync(expectedDevice);

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockDeviceTagService.Setup(service => service.GetAllTagsNames())
                .Returns(expectedDevice.Tags.Select(tag => tag.Name));

            // Act
            var result = await this.lorawanDeviceService.GetDevice(expectedDeviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevice_DeviceNotExist_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync((LorawanDevice)null);

            // Act
            var act = () => this.lorawanDeviceService.GetDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_NewDevice_DeviceCreated()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.lorawanDeviceService.CreateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_DbUpdateExceptionIsThrown_InternalServerErrorExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.lorawanDeviceService.CreateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }
    }
}
