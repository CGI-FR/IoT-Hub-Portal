// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoFixture;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncGreenGrassDevicesJobTests : BackendUnitTest
    {
        private IJob syncGreenGrassDevicesJob;

        private Mock<IAmazonIoT> amazonIoTClient;
        private Mock<IAmazonGreengrassV2> amazonGreenGrass;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDeviceRepository> mockDeviceRepository;
        private Mock<IEdgeDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceRepository = MockRepository.Create<IEdgeDeviceRepository>();
            this.mockDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.amazonIoTClient = MockRepository.Create<IAmazonIoT>();
            this.amazonGreenGrass = MockRepository.Create<IAmazonGreengrassV2>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIoTClient.Object);
            _ = ServiceCollection.AddSingleton(this.amazonGreenGrass.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncGreenGrassDevicesJob>();


            Services = ServiceCollection.BuildServiceProvider();

            this.syncGreenGrassDevicesJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task ExecuteNewEdgeDeviceEdgeDeviceCreated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var newDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 1
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = newDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = newDevice.Id,
                    ThingName = newDevice.Name,
                    ThingTypeName = newDevice.DeviceModel.Name,
                    Version = newDevice.Version,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(newDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(newDevice.Id, d => d.Tags))
                .ReturnsAsync((EdgeDevice)null);

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingDeviceWithHigherVersionDeviceUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 1,
                Tags = new List<DeviceTagValue>()
                {
                    new()
                    {
                        Id = Fixture.Create<string>(),
                        Name = Fixture.Create<string>(),
                        Value = Fixture.Create<string>()
                    }
                }
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    ThingTypeName = existingDevice.DeviceModel.Name,
                    Version = 2,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>())).Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>())).Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingEdgeDeviceWithOlderVersionEdgeDeviceNotUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 2,
                Tags = new List<DeviceTagValue>()
                {
                    new()
                    {
                        Id = Fixture.Create<string>(),
                        Name = Fixture.Create<string>(),
                        Value = Fixture.Create<string>()
                    }
                }
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    ThingTypeName = existingDevice.DeviceModel.Name,
                    Version = 1,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewEdgeDeviceWithDescribeThingErrorSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 2
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.RequestTimeout
                });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithoutThingTypeSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 2
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    Version = 1,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithUnknownThingTypeSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 2
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    ThingTypeName = existingDevice.DeviceModel.Name,
                    Version = 1,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync((EdgeDeviceModel)null);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        public async Task ExecuteNewEdgeDeviceWithAmazonIotExceptionSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<EdgeDeviceModel>();
            var existingDevice = new EdgeDevice
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 2
            };

            var greenGrassDevices = new ListCoreDevicesResponse
            {
                CoreDevices = new List<CoreDevice>()
                {
                    new CoreDevice
                    {
                        CoreDeviceThingName = existingDevice.Name
                    }
                }
            };

            _ = this.amazonGreenGrass.Setup(client => client.ListCoreDevicesAsync(It.IsAny<ListCoreDevicesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(greenGrassDevices);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new AmazonIoTException(""));

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassDevicesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
