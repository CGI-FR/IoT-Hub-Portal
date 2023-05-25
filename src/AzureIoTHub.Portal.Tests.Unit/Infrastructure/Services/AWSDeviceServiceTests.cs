// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System.Net;
    using System.Threading;
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
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Device = Portal.Domain.Entities.Device;

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
            _ = ServiceCollection.AddSingleton<IAWSExternalDeviceService, AWSExternalDeviceService>();
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

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingResponse
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateThingShadowResponse
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

            _ = this.mockAmazonIotClient.Setup(s3 => s3.CreateThingAsync(It.IsAny<CreateThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingResponse
                {
                    ThingId = deviceDto.DeviceID,
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockAmazonIotDataClient.Setup(s3 => s3.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateThingShadowResponse
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
    }
}
