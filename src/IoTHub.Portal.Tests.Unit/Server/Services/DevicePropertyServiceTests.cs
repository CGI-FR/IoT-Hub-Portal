// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0;
    using UnitTests.Bases;

    [TestFixture]
    public class DevicePropertyServiceTests : BackendUnitTest
    {
        private Mock<IExternalDeviceService> mockDeviceService;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;
        private IDevicePropertyService devicePropertyService;

        public override void Setup()
        {
            base.Setup();


            this.mockDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceModelPropertiesService = MockRepository.Create<IDeviceModelPropertiesService>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelPropertiesService.Object);
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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
                    .Throws(new RequestFailedException("test"));
            // Act
            var act = () => this.devicePropertyService.GetProperties("aaa");

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceDoesntHaveModelIdNotExistGetPropertiesShouldThrowResourceNotFoundException()
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
        public async Task WhenDeviceNotExistGetPropertiesShouldThrowResourceNotFoundException()
        {
            // Arrange
            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync((Twin)null);

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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
                .ReturnsAsync(new[]
                    {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            },
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = "bbb",
                                IsWritable = false,
                                Name = "notwritable",
                            }
                        });

            // Act
            var devicePropertyValues = await this.devicePropertyService.GetProperties("aaa");

            // Assert
            _ = devicePropertyValues.Count().Should().Be(2);
            _ = devicePropertyValues.Single(x => x.Name == "writable").Value.Should().Be("ccc");
            _ = devicePropertyValues.Single(x => x.Name == "notwritable").Value.Should().BeNull();

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceNotExistSetPropertiesShouldThrowResourceNotFoundException()
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
        public async Task WhenDeviceDoesntHaveModelIdNotExistSetPropertiesShouldThrowResourceNotFoundException()
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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                    It.Is<Twin>(c => c == twin)))
                .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
                .ReturnsAsync(new[]
                    {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            }
                        });
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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                It.Is<Twin>(c => c == twin)))
            .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
                .ReturnsAsync(new[]
                    {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = "bbb",
                                IsWritable = false,
                                Name = "notwritable",
                            }
                        });
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

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin("aaa"))
                .ReturnsAsync(twin);

            _ = this.mockDeviceService.Setup(c => c.UpdateDeviceTwin(
                It.Is<Twin>(c => c == twin)))
            .ReturnsAsync(twin);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties("bbb"))
                .ReturnsAsync(new[]
                    {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = "bbb",
                                IsWritable = true,
                                Name = "writable",
                            }
                        });

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
