// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Jobs;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Quartz;
    using Shared;
    using UnitTests.Bases;

    public class SyncDevicesJobTests : BackendUnitTest
    {
        private IJob syncDevicesJob;

        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<ILorawanDeviceRepository> mockLorawanDeviceRepository;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockLorawanDeviceRepository = MockRepository.Create<ILorawanDeviceRepository>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();


            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLorawanDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync((Device)null);

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = expectedTwinDevice.DeviceId
                    },
                    new Device
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockLorawanDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<LorawanDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LorawanDevice>
                {
                    new LorawanDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    },
                    new LorawanDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockLorawanDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

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
                Version = 1,
                Tags = new List<DeviceTagValue>
                {
                    new()
                    {
                        Id = Fixture.Create<string>(),
                        Name = Fixture.Create<string>(),
                        Value = Fixture.Create<string>()
                    }
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>()))
                .Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockLorawanDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<LorawanDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LorawanDevice>
                {
                    new LorawanDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

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

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync((LorawanDevice)null);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = expectedTwinDevice.DeviceId
                    },
                    new Device
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockLorawanDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<LorawanDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LorawanDevice>
                {
                    new LorawanDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    },
                    new LorawanDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockLorawanDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

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
                Version = 1,
                Tags = new List<DeviceTagValue>
                {
                    new()
                    {
                        Id = Fixture.Create<string>(),
                        Name = Fixture.Create<string>(),
                        Value = Fixture.Create<string>()
                    }
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

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync(existingLorawanDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLorawanDeviceRepository.Setup(repository => repository.Update(It.IsAny<LorawanDevice>()))
                .Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockLorawanDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<LorawanDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LorawanDevice>
                {
                    new LorawanDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task Execute_MissingModelId_ShouldNotFail()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();
            expectedDeviceModel.SupportLoRaFeatures = true;

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
            };

            _ = this.mockDeviceService
                .Setup(x => x.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
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

            // Act
            await this.syncDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
