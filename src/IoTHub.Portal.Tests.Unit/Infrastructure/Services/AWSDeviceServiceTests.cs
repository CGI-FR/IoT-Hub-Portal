// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
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
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Services.AWS;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Device = Portal.Domain.Entities.Device;
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;
    using System.Threading;
    using IoTHub.Portal.Domain.Exceptions;
    using Shared.Models.v1._0;

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
        private Mock<IExternalDeviceService> mockExternalDeviceService;

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
            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotDataClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfiguration.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
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

            _ = this.mockAmazonIotClient.Setup(service => service.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingResponse()
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotDataClient.Setup(service => service.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
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
        public async Task CreateDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOKForCreateThing()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockAmazonIotClient.Setup(service => service.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));

            //Act
            var result = () => this.awsDeviceService.CreateDevice(deviceDto);

            //Assert
            _ = await result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOKForUpdateThingShadow()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockAmazonIotClient.Setup(service => service.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingResponse()
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotDataClient.Setup(service => service.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIotDataException(It.IsAny<string>()));

            //Act
            var result = () => this.awsDeviceService.CreateDevice(deviceDto);

            //Assert
            _ = await result.Should().ThrowAsync<InternalServerErrorException>();
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

            _ = this.mockAmazonIotClient.Setup(service => service.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingResponse()
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotDataClient.Setup(service => service.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
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

            _ = this.mockAmazonIotClient.Setup(service => service.UpdateThingAsync(It.IsAny<UpdateThingRequest>(), It.IsAny<CancellationToken>()))
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
        public async Task UpdateDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOKForUpdateThing()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            _ = this.mockAmazonIotClient.Setup(service => service.UpdateThingAsync(It.IsAny<UpdateThingRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));

            // Act
            var result = () => this.awsDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await result.Should().ThrowAsync<InternalServerErrorException>();
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

            _ = this.mockAmazonIotClient.Setup(service => service.UpdateThingAsync(It.IsAny<UpdateThingRequest>(), It.IsAny<CancellationToken>()))
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

            _ = this.mockAmazonIotClient.Setup(service => service.UpdateThingAsync(It.IsAny<UpdateThingRequest>(), It.IsAny<CancellationToken>()))
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

            _ = this.mockAmazonIotClient.Setup(service => service.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListThingPrincipalsResponse()
            {
                Principals = new List<string>()
                {
                    Fixture.Create<string>()
                },
                HttpStatusCode = HttpStatusCode.OK
            });

            _ = this.mockAmazonIotClient.Setup(service => service.DetachThingPrincipalAsync(It.IsAny<DetachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DetachThingPrincipalResponse()
            {
                HttpStatusCode = HttpStatusCode.OK
            });

            _ = this.mockAmazonIotClient.Setup(service => service.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
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
        public async Task DeleteDeviceResourceNotFoundExceptionIfDeviceNotExist()
        {
            // Arrange
            var deviceDto = new DeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
               .ReturnsAsync((Device)null);

            // Act
            var response = () => this.awsDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            _ = await response.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOKForListThingPrincipals()
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
                .ReturnsAsync(device);

            _ = this.mockAmazonIotClient.Setup(service => service.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));

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
        public async Task DeleteDeviceShouldThrowExceptionIfFailedWhenDetachThingPrincipal()
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
                .ReturnsAsync(device);

            _ = this.mockAmazonIotClient.Setup(service => service.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListThingPrincipalsResponse()
            {
                Principals = new List<string>()
                {
                    Fixture.Create<string>()
                },
                HttpStatusCode = HttpStatusCode.OK
            });

            _ = this.mockAmazonIotClient.Setup(service => service.DetachThingPrincipalAsync(It.IsAny<DetachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));
            _ = this.mockAmazonIotClient.Setup(service => service.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));

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
        public async Task DeleteDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOKForDeleteThing()
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

            _ = this.mockDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID))
                .ReturnsAsync(device);

            _ = this.mockAmazonIotClient.Setup(service => service.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListThingPrincipalsResponse()
            {
                Principals = new List<string>()
                {
                    Fixture.Create<string>()
                },
                HttpStatusCode = HttpStatusCode.OK
            }); ;

            _ = this.mockAmazonIotClient.Setup(service => service.DetachThingPrincipalAsync(It.IsAny<DetachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DetachThingPrincipalResponse()
            {
                HttpStatusCode = HttpStatusCode.OK
            });

            _ = this.mockAmazonIotClient.Setup(service => service.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIoTException(It.IsAny<string>()));

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

            _ = this.mockAmazonIotClient.Setup(service => service.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingPrincipalsResponse()
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotClient.Setup(service => service.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
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
