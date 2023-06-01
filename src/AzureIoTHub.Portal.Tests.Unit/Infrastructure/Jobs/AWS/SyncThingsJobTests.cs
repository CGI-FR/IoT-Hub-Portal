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
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using AutoFixture;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncThingsJobTests : BackendUnitTest
    {
        private IJob syncThingsJob;

        private Mock<IAmazonIoT> amazonIoTClient;
        private Mock<IAmazonIotData> amazonIoTDataClient;
        private Mock<IDeviceModelImageManager> mockAWSImageManager;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;

        public override void Setup()
        {
            base.Setup();

            this.mockAWSImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.amazonIoTClient = MockRepository.Create<IAmazonIoT>();
            this.amazonIoTDataClient = MockRepository.Create<IAmazonIotData>();

            _ = ServiceCollection.AddSingleton(this.mockAWSImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIoTClient.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIoTDataClient.Object);
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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(newDevice.DeviceModel.Name))
                .Returns(expectedDeviceModel);

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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(existingDevice.DeviceModel.Name))
                .Returns(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>())).Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Update(It.IsAny<Device>())).Verifiable();

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(existingDevice.DeviceModel.Name))
                .Returns(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = HttpStatusCode.OK });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(existingDevice.Id, d => d.Tags))
                .ReturnsAsync(existingDevice);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithDescribeThingErrorSkipped()
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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    ThingTypeName = existingDevice.DeviceModel.Name,
                    Version = 1,
                    HttpStatusCode = HttpStatusCode.RequestTimeout
                });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithoutThingTypeSkipped()
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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = existingDevice.Id,
                    ThingName = existingDevice.Name,
                    Version = 1,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteNewDeviceWithUnknownThingTypeSkipped()
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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(existingDevice.DeviceModel.Name))
                .Returns((DeviceModel)null);

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(existingDevice.DeviceModel.Name))
                .Returns(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse() { HttpStatusCode = thingShadowCode });

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

            _ = this.amazonIoTClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(""));

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }

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

            _ = this.amazonIoTClient.Setup(client => client.ListThingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

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
                .Setup(x => x.GetByName(existingDevice.DeviceModel.Name))
                .Returns(expectedDeviceModel);

            _ = this.amazonIoTDataClient.Setup(client => client.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIotDataException(""));

            _ = this.mockDeviceRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Device, bool>>>(), It.IsAny<CancellationToken>(), d => d.Tags, d => d.Labels))
                .ReturnsAsync(new List<Device>());

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingsJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
