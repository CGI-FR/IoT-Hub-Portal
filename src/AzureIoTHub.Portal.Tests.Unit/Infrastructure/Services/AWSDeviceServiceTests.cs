// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Device = Portal.Domain.Entities.Device;
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class AWSDeviceServiceTests : BackendUnitTest
    {
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAmazonIoT> mockAmazonIotClient;
        private Mock<IAmazonIotData> mockAmazonIotDataClient;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IAWSExternalDeviceService> mockAWSExternalDevicesService;

        private IDeviceService<DeviceDetails> awsDeviceService;

        [SetUp]
        public void SetUp()
        {

            base.Setup();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockAmazonIotClient = MockRepository.Create<IAmazonIoT>();
            this.mockAmazonIotDataClient = MockRepository.Create<IAmazonIotData>();
            this.mockConfiguration = MockRepository.Create<IConfiguration>();
            this.mockAWSExternalDevicesService = MockRepository.Create<IAWSExternalDeviceService>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotDataClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfiguration.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton(this.mockAWSExternalDevicesService.Object);
            _ = ServiceCollection.AddSingleton<IDeviceService<DeviceDetails>, AWSDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.awsDeviceService = Services.GetRequiredService<IDeviceService<DeviceDetails>>();

            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateADeviceShouldReturnAValue()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.CreateDevice(It.IsAny<CreateThingRequest>()))
                .ReturnsAsync(new CreateThingResponse()
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAWSExternalDevicesService.Setup(service => service.UpdateDeviceShadow(It.IsAny<UpdateThingShadowRequest>()))
                .ReturnsAsync(new UpdateThingShadowResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            //Act
            var result = await this.awsDeviceService.CreateDevice(deviceDto);

            //Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceDuplicateExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.CreateDevice(It.IsAny<CreateThingRequest>()))
                .ReturnsAsync(new CreateThingResponse()
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAWSExternalDevicesService.Setup(service => service.UpdateDeviceShadow(It.IsAny<UpdateThingShadowRequest>()))
                .ReturnsAsync(new UpdateThingShadowResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new UniqueConstraintException());

            // Act
            var act = () => this.awsDeviceService.CreateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<UniqueConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceShouldReturnValue()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<UpdateThingRequest>()))
                .ReturnsAsync(new UpdateThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

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
            var result = await this.awsDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceThatNotExistThrowResourceNotFoundException()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<UpdateThingRequest>()))
                .ReturnsAsync(new UpdateThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((Device)null);

            // Act
            var act = () => this.awsDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceWhenDbUpdateExceptionIsRaisedCannotInsertNullExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<UpdateThingRequest>()))
                .ReturnsAsync(new UpdateThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

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
            var act = () => this.awsDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<CannotInsertNullException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var device = new Device
            {
                Id = deviceDto.DeviceID,
                Tags = Fixture.CreateMany<DeviceTagValue>(5).ToList(),
                Labels = Fixture.CreateMany<Label>(5).ToList()
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.DeleteDevice(It.IsAny<DeleteThingRequest>()))
               .ReturnsAsync(new DeleteThingResponse()
               {
                   HttpStatusCode = HttpStatusCode.OK
               });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(device);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
                .ReturnsAsync(device);

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.awsDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceNothingIsDoneIfDeviceNotExist()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.DeleteDevice(It.IsAny<DeleteThingRequest>()))
                .ReturnsAsync(new DeleteThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((Device)null);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
               .ReturnsAsync((Device)null);

            // Act
            await this.awsDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceWhenDbUpdateExceptionIsRaisedDbUpdateExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var device = new Device
            {
                Id = deviceDto.DeviceID,
                Tags = new List<DeviceTagValue>(),
                Labels =  new List<Label>(),
            };

            _ = this.mockAWSExternalDevicesService.Setup(service => service.DeleteDevice(It.IsAny<DeleteThingRequest>()))
                .ReturnsAsync(new DeleteThingResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(device);

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
                .ReturnsAsync(device);

            this.mockDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            var act = () => this.awsDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            _ = await act.Should().ThrowAsync<DbUpdateException>();
            MockRepository.VerifyAll();
        }
    }
}
