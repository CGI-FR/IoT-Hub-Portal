// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Jobs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Jobs;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Quartz;
    using UnitTests.Bases;

    public class SyncDeviceJobTests : BackendUnitTest
    {
        private IJob syncDevicesJob;

        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<ILorawanDeviceRepository> mockLorawanDeviceRepository;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockLorawanDeviceRepository = MockRepository.Create<ILorawanDeviceRepository>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();


            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLorawanDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncDevicesJob>();

            Services = ServiceCollection.BuildServiceProvider();

            this.syncDevicesJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task Execute_NewDevice_DeviceCreated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            expectedDeviceModel.SupportLoRaFeatures = false;

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                }
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });


            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId))
                .ReturnsAsync((Device)null);

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task Execute_ExistingDeviceWithGreeterVersion_DeviceUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            expectedDeviceModel.SupportLoRaFeatures = false;

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                },
                Version = 2
            };

            var existingDevice = new Device
            {
                Id = expectedTwinDevice.DeviceId,
                Version = 1
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });


            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId))
                .ReturnsAsync(existingDevice);

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task Execute_NewLorawanDevice_LorawanDeviceCreated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            expectedDeviceModel.SupportLoRaFeatures = true;

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                }
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });


            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId))
                .ReturnsAsync((LorawanDevice)null);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task Execute_ExistingLorawanDeviceWithGreeterVersion_LorawanDeviceUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            expectedDeviceModel.SupportLoRaFeatures = true;

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                },
                Version = 2
            };

            var existingLorawanDevice = new LorawanDevice
            {
                Id = expectedTwinDevice.DeviceId,
                Version = 1
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(x => x == "LoRa Concentrator"),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });


            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId))
                .ReturnsAsync(existingLorawanDevice);

            this.mockLorawanDeviceRepository.Setup(repository => repository.Update(It.IsAny<LorawanDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
