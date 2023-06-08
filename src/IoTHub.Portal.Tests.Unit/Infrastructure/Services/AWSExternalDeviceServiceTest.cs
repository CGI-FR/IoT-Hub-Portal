// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System.Collections.Generic;
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
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Application.Services.AWS;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Services.AWS;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Domain.Entities;

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

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingPrincipalsResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Principals = Fixture.Create<List<string>>()
                });

            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.DetachThingPrincipalAsync(It.IsAny<DetachThingPrincipalRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DetachThingPrincipalResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

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
            _ = this.mockAmazonIotClient.Setup(iotClient => iotClient.ListThingPrincipalsAsync(It.IsAny<ListThingPrincipalsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListThingPrincipalsResponse
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

        [Test]
        public async Task IsEdgeThingTypeReturnTrue()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIotClient.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "true"
                                    }
                    }
                });

            //Act
            var result = await this.awsExternalDeviceService.IsEdgeThingType(thingType);

            //Assert
            _ = result.Should().BeTrue();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task IsNotEdgeThingTypeReturnFalse()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIotClient.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "false"
                                    }
                    }
                });

            //Act
            var result = await this.awsExternalDeviceService.IsEdgeThingType(thingType);

            //Assert
            _ = result.Should().BeFalse();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task EdgeThingTypeNullReturnNull()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIotClient.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ListTagsForResourceResponse)null);

            //Act
            var result = await this.awsExternalDeviceService.IsEdgeThingType(thingType);

            //Assert
            _ = result.Should().BeNull();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task EdgeThingTypeNotBooleanReturnNull()
        {
            // Arrange
            var thingType = new DescribeThingTypeResponse()
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            _ = this.mockAmazonIotClient.Setup(client => client.ListTagsForResourceAsync(It.Is<ListTagsForResourceRequest>(c => c.ResourceArn == thingType.ThingTypeArn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListTagsForResourceResponse
                {
                    NextToken = Fixture.Create<string>(),
                    Tags = new List<Tag>
                    {
                                    new Tag
                                    {
                                        Key = "iotEdge",
                                        Value = "123"
                                    }
                    }
                });

            //Act
            var result = await this.awsExternalDeviceService.IsEdgeThingType(thingType);

            //Assert
            _ = result.Should().BeNull();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllThingShouldReturnsAllAWSThings()
        {

            //Arrange
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

            _ = this.mockAmazonIotClient.Setup(client => client.ListThingsAsync(It.IsAny<ListThingsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingsListing);

            _ = this.mockAmazonIotClient.Setup(client => client.DescribeThingAsync(It.IsAny<DescribeThingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingResponse()
                {
                    ThingId = newDevice.Id,
                    ThingName = newDevice.Name,
                    ThingTypeName = newDevice.DeviceModel.Name,
                    Version = newDevice.Version,
                    HttpStatusCode = HttpStatusCode.OK
                });


            //Act
            var result = await this.awsExternalDeviceService.GetAllThings();

            //Assert
            MockRepository.VerifyAll();
        }
    }
}
