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
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Quartz;
    using Shared;

    public class SyncEdgeDeviceJobTest : BackendUnitTest
    {
        private IJob syncEdgeDeviceJob;

        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IEdgeDeviceRepository> mockEdgeDeviceRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<IEdgeDeviceModelRepository> mockEdgeDeviceModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDevicesService> mockEdgeDevicesService;
        private Mock<ILogger<SyncEdgeDeviceJob>> mockLogger;

        public override void Setup()
        {
            base.Setup();

            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockEdgeDeviceRepository = MockRepository.Create<IEdgeDeviceRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockEdgeDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeDevicesService = MockRepository.Create<IEdgeDevicesService>();
            this.mockLogger = MockRepository.Create<ILogger<SyncEdgeDeviceJob>>();

            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDevicesService.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncEdgeDeviceJob>();

            Services = ServiceCollection.BuildServiceProvider();

            this.syncEdgeDeviceJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task ExecuteNewEdgeDeviceDeviceCreated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>(),
                    ["test"] = Fixture.Create<string>()
                },
                Capabilities = new DeviceCapabilities{ IotEdge = true }
            };

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetAllEdgeDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetDeviceTwinWithModule(It.Is<string>(c => c.Equals(expectedTwinDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedTwinDevice);

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetDeviceTwinWithEdgeHubModule(It.Is<string>(c => c.Equals(expectedTwinDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedTwinDevice);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync(value: null);

            _ = this.mockEdgeDeviceRepository.Setup(x => x.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    },
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockEdgeDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncEdgeDeviceJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingEdgeDeviceWithGreeterVersionDeviceUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                },
                Capabilities = new DeviceCapabilities{ IotEdge = true },
                Version = 2
            };

            var existingDevice = new EdgeDevice
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

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetAllEdgeDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string>(),
                    It.Is<int>(x => x == 100)))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>
                    {
                        expectedTwinDevice
                    },
                    TotalItems = 1
                });

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetDeviceTwinWithModule(It.Is<string>(c => c.Equals(expectedTwinDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedTwinDevice);

            _ = this.mockExternalDeviceService
                .Setup(x => x.GetDeviceTwinWithEdgeHubModule(It.Is<string>(c => c.Equals(expectedTwinDevice.DeviceId, StringComparison.Ordinal))))
                .ReturnsAsync(expectedTwinDevice);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByIdAsync(expectedDeviceModel.Id))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetByIdAsync(expectedTwinDevice.DeviceId, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>()))
                .Verifiable();

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncEdgeDeviceJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenErrorOccursJobShouldContinueToNextDevice()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var fakeTwin = Fixture.Create<Twin>();

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                },
                Capabilities = new DeviceCapabilities{ IotEdge = true },
                Version = 2
            };

            _ = this.mockExternalDeviceService
              .Setup(x => x.GetAllEdgeDevice(
                  It.IsAny<string>(),
                  It.IsAny<string>(),
                  It.IsAny<bool?>(),
                  It.IsAny<string>(),
                  It.Is<int>(x => x == 100)))
              .ReturnsAsync(new PaginationResult<Twin>
              {
                  Items = new List<Twin>
                  {
                      expectedTwinDevice,
                      fakeTwin
                  },
                  TotalItems = 2
              });

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceModelRepository.Setup(c => c.GetByIdAsync(It.IsAny<string>()))
                .Throws<NotImplementedException>();

            // Act
            await this.syncEdgeDeviceJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenModelIsNotFoundShouldContinueToNextDevice()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var fakeTwin = Fixture.Create<Twin>();

            var expectedTwinDevice = new Twin
            {
                DeviceId = Fixture.Create<string>(),
                Tags = new TwinCollection
                {
                    ["modelId"] = expectedDeviceModel.Id,
                    ["deviceName"] = Fixture.Create<string>()
                },
                Capabilities = new DeviceCapabilities{ IotEdge = true },
                Version = 2
            };

            _ = this.mockExternalDeviceService
              .Setup(x => x.GetAllEdgeDevice(
                  It.IsAny<string>(),
                  It.IsAny<string>(),
                  It.IsAny<bool?>(),
                  It.IsAny<string>(),
                  It.Is<int>(x => x == 100)))
              .ReturnsAsync(new PaginationResult<Twin>
              {
                  Items = new List<Twin>
                  {
                      expectedTwinDevice,
                      expectedTwinDevice,
                  },
                  TotalItems = 2
              });

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = expectedTwinDevice.DeviceId
                    }
                });

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockEdgeDeviceModelRepository.Setup(c => c.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((EdgeDeviceModel)null);

            // Act
            await this.syncEdgeDeviceJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
            this.mockEdgeDeviceModelRepository.Verify(c => c.GetByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task WhenNothinWorksJobShouldNotThrowAnException()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            // Act
            await this.syncEdgeDeviceJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
