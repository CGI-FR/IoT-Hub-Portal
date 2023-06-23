// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
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
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Infrastructure.Jobs.AWS;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncThingsJobTests : BackendUnitTest
    {
        private IJob syncThingsJob;

        private Mock<IAmazonIoT> amazonIoTClient;
        private Mock<IAmazonIotData> amazonIoTDataClient;
        private Mock<IAmazonGreengrassV2> amazonGreenGrass;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IEdgeDeviceRepository> mockEdgeDeviceRepository;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IEdgeDeviceModelRepository> mockEdgeDeviceModelRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<IExternalDeviceService> externalDeviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockEdgeDeviceRepository = MockRepository.Create<IEdgeDeviceRepository>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockEdgeDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.amazonIoTClient = MockRepository.Create<IAmazonIoT>();
            this.amazonIoTDataClient = MockRepository.Create<IAmazonIotData>();
            this.amazonGreenGrass = MockRepository.Create<IAmazonGreengrassV2>();
            this.externalDeviceService = MockRepository.Create<IExternalDeviceService>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIoTClient.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIoTDataClient.Object);
            _ = ServiceCollection.AddSingleton(this.amazonGreenGrass.Object);
            _ = ServiceCollection.AddSingleton(this.externalDeviceService.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncThingsJob>();


            Services = ServiceCollection.BuildServiceProvider();

            this.syncThingsJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task ExecuteNewDeviceDeviceCreated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var newDevice = new Device
            {
                Id = Fixture.Create<string>(),
                Name = Fixture.Create<string>(),
                DeviceModel = expectedDeviceModel,
                DeviceModelId = expectedDeviceModel.Id,
                Version = 1,
            };

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = newDevice.Id,
                        ThingName = newDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(newDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(newDevice.Id, d => d.Tags))
                .ReturnsAsync((Device)null);

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockEdgeDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingDeviceWithHigherVersionDeviceUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 2,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>())).Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>())).Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingDeviceWithOlderVersionDeviceNotUpdated()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public async Task ExecuteNewDeviceWithUnknownIsEdgeTagSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync((bool?)null);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithUnknownDeviceModelSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync((DeviceModel)null);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.BadRequest)]
        public async Task ExecuteNewDeviceWithoutThingShadowSkipped(HttpStatusCode thingShadowCode)
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = thingShadowCode });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithAmazonIotExceptionSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.BadGateway
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);


            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithAmazonIotDataExceptionSkipped()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var expectedDeviceModel = Fixture.Create<DeviceModel>();

            var existingDevice = new Device
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            _ = this.mockDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new AmazonIotDataException(""));

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
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

            var thingsListing = new ListThingsResponse
            {
                Things = new List<ThingAttribute>()
                {
                    new ThingAttribute
                    {
                        ThingName = newDevice.Name
                    }
                }
            };

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = newDevice.Id,
                        ThingName = newDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 1,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(true);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByNameAsync(newDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonGreenGrass.Setup(client => client.GetCoreDeviceAsync(It.IsAny<GetCoreDeviceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoreDeviceResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(newDevice.Id, d => d.Tags))
                .ReturnsAsync((EdgeDevice)null);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockEdgeDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteExistingEdgeDeviceWithHigherVersionEdgeDeviceUpdated()
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

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 2,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(true);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonGreenGrass.Setup(client => client.GetCoreDeviceAsync(It.IsAny<GetCoreDeviceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoreDeviceResponse() { HttpStatusCode = HttpStatusCode.OK, Status = CoreDeviceStatus.HEALTHY });

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>())).Verifiable();

            this.mockEdgeDeviceRepository.Setup(repository => repository.Update(It.IsAny<EdgeDevice>())).Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

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
                Version = 2
            };

            var thingsListing = new ListThingsResponse
            {
                Things = new List<ThingAttribute>()
                {
                    new ThingAttribute
                    {
                        ThingName = existingDevice.Name
                    }
                }
            };

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 2,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(true);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonGreenGrass.Setup(client => client.GetCoreDeviceAsync(It.IsAny<GetCoreDeviceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoreDeviceResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewEdgeDeviceWithUnknownEdgeDeviceModelSkipped()
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

            var thingsListing = new ListThingsResponse
            {
                Things = new List<ThingAttribute>()
                {
                    new ThingAttribute
                    {
                        ThingName = existingDevice.Name
                    }
                }
            };

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = existingDevice.Id,
                        ThingName = existingDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 2,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(true);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByNameAsync(existingDevice.DeviceModel.Name))
                .ReturnsAsync((EdgeDeviceModel)null);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewEdgeDeviceWithoutCoreDeviceDeviceCreatedWithDisconnectedState()
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

            var thingsListing = new ListThingsResponse
            {
                Things = new List<ThingAttribute>()
                {
                    new ThingAttribute
                    {
                        ThingName = newDevice.Name
                    }
                }
            };

            var listDescribeThingResponse = new List<DescribeThingResponse>
                {
                    new DescribeThingResponse
                    {
                        ThingId = newDevice.Id,
                        ThingName = newDevice.Name,
                        ThingTypeName = expectedDeviceModel.Name,
                        Version = 2,
                        HttpStatusCode = HttpStatusCode.OK
                    }
                };

            _ = this.externalDeviceService.Setup(client => client.GetAllThing())
                .ReturnsAsync(listDescribeThingResponse);

            _ = this.externalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(true);

            _ = this.mockEdgeDeviceModelRepository
                .Setup(x => x.GetByNameAsync(newDevice.DeviceModel.Name))
                .ReturnsAsync(expectedDeviceModel);

            _ = this.amazonGreenGrass.Setup(client => client.GetCoreDeviceAsync(It.IsAny<GetCoreDeviceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoreDeviceResponse() { HttpStatusCode = HttpStatusCode.NotFound });

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.GetByIdAsync(newDevice.Id, d => d.Tags))
                .ReturnsAsync((EdgeDevice)null);

            _ = this.mockEdgeDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<EdgeDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>
                {
                    new Device
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockEdgeDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<EdgeDevice, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<EdgeDevice>
                {
                    new EdgeDevice
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            this.mockEdgeDeviceRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
