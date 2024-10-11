// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System.IO;
    using System.Text;
    using AutoMapper;
    using Azure;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Services.AWS;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Device = Portal.Domain.Entities.Device;
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class AWSDevicePropertyServiceTests : BackendUnitTest
    {
        private Mock<IDeviceRepository> mockDeviceRepository;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAmazonIoT> mockAmazonIotClient;
        private Mock<IAmazonIotData> mockAmazonIotDataClient;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IAmazonGreengrassV2> mockGreenGrass;

        private IDevicePropertyService awsDevicePropertyService;

        [SetUp]
        public void SetUp()
        {

            base.Setup();
            this.mockDeviceRepository = MockRepository.Create<IDeviceRepository>();
            this.mockDeviceModelPropertiesService = MockRepository.Create<IDeviceModelPropertiesService>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockAmazonIotClient = MockRepository.Create<IAmazonIoT>();
            this.mockAmazonIotDataClient = MockRepository.Create<IAmazonIotData>();
            this.mockConfiguration = MockRepository.Create<IConfiguration>();
            this.mockGreenGrass = MockRepository.Create<IAmazonGreengrassV2>();


            _ = ServiceCollection.AddSingleton(this.mockDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelPropertiesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIotDataClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfiguration.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton(this.mockGreenGrass.Object);
            _ = ServiceCollection.AddSingleton<IDevicePropertyService, AWSDevicePropertyService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.awsDevicePropertyService = Services.GetRequiredService<IDevicePropertyService>();

            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task WhenDeviceIdNotExistGetPropertiesShouldThrowResourceNotFoundException()
        {
            // Arrange
            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync("aaa"))
                .ReturnsAsync((Device)null);

            // Act
            var result =  () => this.awsDevicePropertyService.GetProperties("aaa");

            // Assert
            _ = await result.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPropertiesShouldThrowInternalServerErrorExceptionWhenHttpStatusCodeIsNotOKForGetThingShadow()
        {
            // Arrange
            var device = new Device()
            {
                Id = "aaa",
                DeviceModelId = "bbb"
            };

            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync("aaa"))
                .ReturnsAsync(device);

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIotDataException("Unable to get the thing shadow due to an error in the Amazon IoT API"));

            // Act
            var act = () => this.awsDevicePropertyService.GetProperties(device.Id);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPropertiesShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingProperties()
        {
            // Arrange
            var device = new Device()
            {
                Id = "aaa",
                DeviceModelId = "bbb"
            };

            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync("aaa"))
                .ReturnsAsync(device);

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(device.DeviceModelId))
                    .Throws(new RequestFailedException("test"));

            // Act
            var act = () => this.awsDevicePropertyService.GetProperties(device.Id);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPropertiesShouldReturnDeviceProperties()
        {
            // Arrange
            var device = new Device()
            {
                Id = "aaa",
                DeviceModelId = "bbb"
            };

            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync("aaa"))
                .ReturnsAsync(device);

            var json = new
            {
                state = new
                {
                    desired = new
                    {
                        writable = "aaa",
                        notwritable = "bbb"
                    },
                    reported = new
                    {
                        writable = "ccc",
                        notwritable = "ddd"
                    }
                }
            };

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)));

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.GetThingShadowAsync(It.IsAny<GetThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetThingShadowResponse
                {
                    Payload = ms,
                    HttpStatusCode = HttpStatusCode.OK
                }); ;

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(device.DeviceModelId))
                .ReturnsAsync(new[]
                    {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = device.DeviceModelId,
                                IsWritable = true,
                                Name = "writable",
                            },
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = device.DeviceModelId,
                                IsWritable = false,
                                Name = "notwritable",
                            }
                        });

            // Act
            var devicePropertyValues = await this.awsDevicePropertyService.GetProperties(device.Id);

            // Assert
            _ = devicePropertyValues.Count().Should().Be(2);
            _ = devicePropertyValues.Single(x => x.Name == "writable").Value.Should().Be("aaa");
            _ = devicePropertyValues.Single(x => x.Name == "notwritable").Value.Should().Be("ddd");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceIdNotExistSetPropertiesShouldThrowResourceNotFoundException()
        {
            // Arrange
            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync("aaa"))
                .ReturnsAsync((Device)null);

            // Act
            var result =  () => this.awsDevicePropertyService.SetProperties("aaa", null);

            // Assert
            _ = await result.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingProperties()
        {
            // Arrange
            var device = new Device()
            {
                Id = "aaa",
                DeviceModelId = "bbb"
            };

            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync(device.Id))
                .ReturnsAsync(device);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(device.DeviceModelId))
                    .Throws(new RequestFailedException("test"));

            // Act
            var act = () => this.awsDevicePropertyService.SetProperties(device.Id, null);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldThrowInternalServerErrorExceptionWhenHttpStatusIsNotOKForUpdateThingShadow()
        {
            // Arrange
            var device = new Device()
            {
                Id = "aaa",
                DeviceModelId = "bbb"
            };

            _ = this.mockDeviceRepository.Setup(c => c.GetByIdAsync(device.Id))
                .ReturnsAsync(device);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(device.DeviceModelId))
                    .ReturnsAsync(Enumerable.Empty<DeviceModelProperty>());

            _ = this.mockAmazonIotDataClient.Setup(iotDataClient => iotDataClient.UpdateThingShadowAsync(It.IsAny<UpdateThingShadowRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonIotDataException("Unable to upadate the thing shadow due to an error in the Amazon IoT API"));


            // Act
            var act = () => this.awsDevicePropertyService.SetProperties(device.Id, null);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }
    }
}
