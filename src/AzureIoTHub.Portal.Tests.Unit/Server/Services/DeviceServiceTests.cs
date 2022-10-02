// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using UnitTests.Bases;
    using NUnit.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using AzureIoTHub.Portal.Server.Services;
    using Moq;
    using Portal.Domain;
    using Portal.Domain.Repositories;
    using Portal.Server.Managers;
    using Portal.Server.Mappers;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain.Entities;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using FluentAssertions;
    using AzureIoTHub.Portal.Domain.Exceptions;

    [TestFixture]
    public class DeviceServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceRepository> mockDeviceRepository;
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
            this.mockExternalDevicesService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceTwinMapper = MockRepository.Create<IDeviceTwinMapper<DeviceListItem, DeviceDetails>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDevicesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IDeviceService<DeviceDetails>, DeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceService = Services.GetRequiredService<IDeviceService<DeviceDetails>>();
        }

        [Test]
        public async Task GetDevice_ExistingDevice_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<Device>();

            var expectedImageUri = Fixture.Create<Uri>();
            var expectedDeviceDto = Mapper.Map<DeviceDetails>(expectedDevice);
            expectedDeviceDto.ImageUrl = expectedImageUri;

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceDto.DeviceID))
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync((Device)null);

            // Act
            var act = () => this.deviceService.GetDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }
    }
}
