// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Controllers.V10;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelPropertiesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DeviceModelPropertiesController>> mockLogger;
        private Mock<IMapper> mockMapper;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelPropertiesController>>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
            this.mockDeviceModelPropertiesService = this.mockRepository.Create<IDeviceModelPropertiesService>();
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(modelId))
                    .ReturnsAsync(new[]
                        {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = modelId
                            }
                        });

            _ = this.mockMapper.Setup(c => c.Map<DeviceProperty>(It.Is<DeviceModelProperty>(x => x.ModelId == modelId)))
                .Returns((DeviceModelProperty x) => new DeviceProperty
                {
                    Name = x.Name,
                });

            // Act
            var response = await deviceModelPropertiesController.GetProperties(modelId);

            // Assert
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as ObjectResult;

            Assert.NotNull(okObjectResult);
            Assert.IsAssignableFrom<List<DeviceProperty>>(okObjectResult.Value);
            var result = (List<DeviceProperty>)okObjectResult.Value;

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelNotExistsGetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(It.IsAny<string>()))
                .Throws(new ResourceNotFoundException("Not Found"));

            // Act
            var response = await deviceModelPropertiesController.GetProperties(Guid.NewGuid().ToString());

            // Assert
            Assert.IsAssignableFrom<NotFoundObjectResult>(response.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
            var modelId = Guid.NewGuid().ToString();

            var properties = new[]
            {
                new DeviceProperty
                {
                    DisplayName =Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.SavePropertiesForModel(modelId, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockMapper.Setup(c => c.Map(
                It.IsAny<DeviceProperty>(),
                It.IsAny<Action<IMappingOperationOptions<object, DeviceModelProperty>>>()))
                .Returns((DeviceProperty x, Action<IMappingOperationOptions<object, DeviceModelProperty>> _) => new DeviceModelProperty
                {
                    Name = x.Name,
                    ModelId = modelId
                });

            // Act
            var result = await deviceModelPropertiesController.SetProperties(modelId, properties);

            // Assert
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldThrowProblemDetailsExceptionWhenModelIsNotValid()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();

            var properties = new[]
            {
                new DeviceProperty()
            };

            deviceModelPropertiesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var act = () => deviceModelPropertiesController.SetProperties(Guid.NewGuid().ToString(), properties);

            // Assert
            _ = await act.Should().ThrowAsync<ProblemDetailsException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelNotExistsSetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.SavePropertiesForModel(It.IsAny<string>(), It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Throws(new ResourceNotFoundException("Not Found"));

            // Act
            var result = await deviceModelPropertiesController.SetProperties(Guid.NewGuid().ToString(), Array.Empty<DeviceProperty>());

            // Assert
            Assert.IsAssignableFrom<NotFoundObjectResult>(result);
            this.mockRepository.VerifyAll();
        }

        private DeviceModelPropertiesController CreateDeviceModelPropertiesController()
        {
            return new DeviceModelPropertiesController(
                this.mockMapper.Object,
                this.mockDeviceModelPropertiesService.Object);
        }
    }
}
