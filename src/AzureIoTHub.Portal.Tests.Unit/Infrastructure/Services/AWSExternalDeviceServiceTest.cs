// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
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
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AWSExternalDeviceServiceTest : BackendUnitTest
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
        private Mock<IAmazonGreengrassV2> mockGreenGrass;

        private IAWSExternalDeviceService awsExternalDeviceService;

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
            this.mockGreenGrass = MockRepository.Create<IAmazonGreengrassV2>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotDataClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfiguration.Object);
            _ = ServiceCollection.AddSingleton(this.mockGreenGrass.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IAWSExternalDeviceService, AWSExternalDeviceService>();
            _ = ServiceCollection.AddSingleton<IDeviceService<DeviceDetails>, AWSDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.awsExternalDeviceService = Services.GetRequiredService<IAWSExternalDeviceService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task GetDeviceShouldReturnAValue()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            var expected = new DescribeThingResponse
            {
                ThingId = deviceDto.DeviceID,
                ThingName = deviceDto.DeviceName,
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.GetDevice(deviceDto.DeviceName);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task GetDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse
                {
                    ThingId = deviceDto.DeviceID,
                    ThingName = deviceDto.DeviceName,
                    HttpStatusCode = HttpStatusCode.BadRequest
                });

            //Act
            var result = () => this.awsExternalDeviceService.GetDevice(deviceDto.DeviceName);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }

        [Test]
        public async Task CreateDeviceShouldReturnAValue()
        {
            // Arrange
            var createThingRequest = new CreateThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            var expected = new CreateThingResponse
            {
                ThingName = createThingRequest.ThingName,
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.CreateThingAsync(createThingRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.CreateDevice(createThingRequest);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task CreateDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var createThingRequest = new CreateThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            var expected = new CreateThingResponse
            {
                ThingName = createThingRequest.ThingName,
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.CreateThingAsync(createThingRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = () => this.awsExternalDeviceService.CreateDevice(createThingRequest);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }

        [Test]
        public async Task UpdateDeviceShouldReturnAValue()
        {
            // Arrange
            var updateThingRequest = new UpdateThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            var expected = new UpdateThingResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.UpdateThingAsync(updateThingRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.UpdateDevice(updateThingRequest);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task UpdateDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var updateThingRequest = new UpdateThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            var expected = new UpdateThingResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.UpdateThingAsync(updateThingRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = () => this.awsExternalDeviceService.UpdateDevice(updateThingRequest);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }

        [Test]
        public async Task DeleteDeviceShouldReturnAValue()
        {
            // Arrange
            var deleteThingRequest = new DeleteThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            var expected = new DeleteThingResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.DeleteDevice(deleteThingRequest);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task DeleteDeviceShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var deleteThingRequest = new DeleteThingRequest()
            {
                ThingName = Fixture.Create<string>(),
            };

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.DeleteThingAsync(It.IsAny<DeleteThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteThingResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest
                });

            //Act
            var result = () => this.awsExternalDeviceService.DeleteDevice(deleteThingRequest);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }

        [Test]
        public async Task GetDeviceShadowShouldReturnAValue()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            var expected = new GetThingShadowResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.GetDeviceShadow(deviceDto.DeviceName);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task GetDeviceShadowShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };

            var expected = new GetThingShadowResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = () => this.awsExternalDeviceService.GetDeviceShadow(deviceDto.DeviceName);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }

        [Test]
        public async Task UpdateDeviceShadowShouldReturnAValue()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };
            var updateThingShadowRequest = new UpdateThingShadowRequest()
            {
                ThingName = deviceDto.DeviceName
            };

            var expected = new UpdateThingShadowResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = await this.awsExternalDeviceService.UpdateDeviceShadow(updateThingShadowRequest);

            //Assert
            _ = result.Should().BeEquivalentTo(expected);
            MockRepository.VerifyAll();
        }

        [Test]
        public Task UpdateDeviceShadowShouldThrowInternalServerErrorIfHttpStatusCodeIsNotOK()
        {
            // Arrange
            var deviceDto = new DeviceDetails()
            {
                DeviceID = Fixture.Create<string>(),
                DeviceName = Fixture.Create<string>(),
            };
            var updateThingShadowRequest = new UpdateThingShadowRequest()
            {
                ThingName = deviceDto.DeviceName
            };

            var expected = new UpdateThingShadowResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            //Act
            var result = () => this.awsExternalDeviceService.UpdateDeviceShadow(updateThingShadowRequest);

            //Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
            return Task.CompletedTask;
        }
    }
}
