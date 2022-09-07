// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices.Shared;
    using Models.v10;
    using Portal.Server.Entities;
    using Portal.Server.Factories;
    using Portal.Server.Helpers;

    [TestFixture]
    public class DevicePropertyServiceTests : BackendUnitTest
    {
        private Mock<IDeviceService> mockDeviceService;
        private Mock<ITableClientFactory> mockTableClientFactory;

        private IDevicePropertyService devicePropertyService;

        public override void Setup()
        {
            base.Setup();


            this.mockDeviceService = MockRepository.Create<IDeviceService>();
            this.mockTableClientFactory = MockRepository.Create<ITableClientFactory>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockTableClientFactory.Object);
            _ = ServiceCollection.AddSingleton<IDevicePropertyService, DevicePropertyService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.devicePropertyService = Services.GetRequiredService<IDevicePropertyService>();
        }

        [Test]
        public async Task GetPropertiesShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingProperties()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");
            DeviceHelper.SetDesiredProperty(twin, "writable", "ccc");

            var mockTableClient = MockRepository.Create<TableClient>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            // Act
            var act = () => this.devicePropertyService.GetProperties("aaa");

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesntHaveModelIdNotExistGetPropertiesShouldReturnBadRequest()
        {
            // Arrange
            var twin = new Twin();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            // Act
            var act = () => this.devicePropertyService.GetProperties("aaa");

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPropertiesShouldReturnDeviceProperties()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");
            DeviceHelper.SetDesiredProperty(twin, "writable", "ccc");

            var mockTableClient = MockRepository.Create<TableClient>();
            var mockResponse = MockRepository.Create<Response>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            },
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "bbb",
                                IsWritable = false,
                                Name = "notwritable",
                            }
                        }, null, mockResponse.Object)
                    }));

            // Act
            var devicePropertyValues = await this.devicePropertyService.GetProperties("aaa");

            // Assert
            _ = devicePropertyValues.Count().Should().Be(2);
            _ = devicePropertyValues.Single(x => x.Name == "writable").Value.Should().Be("ccc");
            _ = devicePropertyValues.Single(x => x.Name == "notwritable").Value.Should().BeNull();

            MockRepository.VerifyAll();
        }
        [Test]
        public async Task WhenDeviceNotExistSetPropertiesShouldReturnNotFound()
        {
            // Arrange
            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync((Twin)null);

            // Act
            var act = () => this.devicePropertyService.SetProperties("aaa", null);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesntHaveModelIdNotExistSetPropertiesShouldReturnBadRequest()
        {
            // Arrange
            var twin = new Twin();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            // Act
            var act = () => this.devicePropertyService.SetProperties("aaa", null);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldUpdateDesiredProperties()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");

            var mockTableClient = MockRepository.Create<TableClient>();
            var mockResponse = MockRepository.Create<Response>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                    It.Is<Twin>(c => c == twin)))
                .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            }
                        }, null, mockResponse.Object)
                    }));

            // Act
            await this.devicePropertyService.SetProperties("aaa", new[]
            {
                new DevicePropertyValue
                {
                    Name = "writable",
                    Value = "ccc"
                }
            });

            // Assert
            Assert.AreEqual("ccc", twin.Properties.Desired["writable"].ToString());

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingProperties()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");

            var mockTableClient = MockRepository.Create<TableClient>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            // Act
            var act = () => this.devicePropertyService.SetProperties("aaa", new[]
            {
                new DevicePropertyValue
                {
                    Name = "writable",
                    Value = "ccc"
                }
            });

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenPropertyNotWrittableSetPropertiesShouldNotUpdateDesiredProperty()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");

            var mockTableClient = MockRepository.Create<TableClient>();
            var mockResponse = MockRepository.Create<Response>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                It.Is<Twin>(c => c == twin)))
            .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "bbb",
                                IsWritable = false,
                                Name = "notwritable",
                            }
                        }, null, mockResponse.Object)
                    }));

            // Act
            await this.devicePropertyService.SetProperties("aaa", new[]
            {
                new DevicePropertyValue
                {
                    Name = "notwritable",
                    Value = "ccc"
                }
            });

            // Assert
            Assert.IsFalse(twin.Properties.Desired.Contains("notwritable"));

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenPropertyNotInModelSetPropertiesShouldNotUpdateDesiredProperty()
        {
            // Arrange
            var twin = new Twin();

            DeviceHelper.SetTagValue(twin, "ModelId", "bbb");

            var mockTableClient = MockRepository.Create<TableClient>();
            var mockResponse = MockRepository.Create<Response>();

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                It.Is<Twin>(c => c == twin)))
            .ReturnsAsync(twin);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(mockTableClient.Object);

            _ = mockTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == "PartitionKey eq 'bbb'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            }
                        }, null, mockResponse.Object)
                    }));

            // Act
            await this.devicePropertyService.SetProperties("aaa", new[]
            {
                new DevicePropertyValue
                {
                    Name = "unknown",
                    Value = "eee"
                }
            });

            // Assert
            Assert.IsFalse(twin.Properties.Desired.Contains("unknown"));

            MockRepository.VerifyAll();
        }
    }
}
