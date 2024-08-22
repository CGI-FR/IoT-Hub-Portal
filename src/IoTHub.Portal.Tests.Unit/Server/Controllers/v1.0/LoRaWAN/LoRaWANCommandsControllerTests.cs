// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using Portal.Server.Controllers.v1._0.LoRaWAN;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaWANCommandsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILoRaWANCommandService> mockLoRaWANCommandService ;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLoRaWANCommandService = this.mockRepository.Create<ILoRaWANCommandService>();
        }

        [Test]
        public async Task PostShouldCreateCommand()
        {
            // Arrange
            var command = new DeviceModelCommandDto
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceModelCommandsController = CreateDeviceModelCommandsController();

            _ = this.mockLoRaWANCommandService.Setup(c => c.PostDeviceModelCommands(It.IsAny<string>(), new[] { command }))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceModelCommandsController.Post(Guid.NewGuid().ToString(), new[] { command });

            // Assert 
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostNullModelIdShouldThrowArgumentNullException()
        {
            //Arrange
            var command = new DeviceModelCommandDto
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceModelCommandsController = CreateDeviceModelCommandsController();

            // Act
            var act = () => deviceModelCommandsController.Post(null, new[] { command });

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostNullCommandsShouldThrowArgumentNullException()
        {
            // Assert
            var deviceModelCommandsController = CreateDeviceModelCommandsController();

            // Act
            var act = () => deviceModelCommandsController.Post(Guid.NewGuid().ToString(), null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetShouldReturnDeviceModelCommands()
        {
            // Arrange
            var command = new DeviceModelCommandDto
            {
                Name = Guid.NewGuid().ToString()
            };

            var deviceModelCommandsController = CreateDeviceModelCommandsController();

            _ = this.mockLoRaWANCommandService.Setup(c => c.GetDeviceModelCommandsFromModel(It.IsAny<string>()))
                .ReturnsAsync(new[] { command });


            // Act
            var response = await deviceModelCommandsController.Get(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            var okResult = (OkObjectResult)response.Result;

            Assert.IsNotNull(okResult);
            Assert.IsAssignableFrom<DeviceModelCommandDto[]>(okResult.Value);

            var result = (DeviceModelCommandDto[])okResult.Value;
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetNullModelIdShouldThrowArgumentNullException()
        {
            // Arrange
            var deviceModelCommandsController = CreateDeviceModelCommandsController();

            // Act
            var act = () => deviceModelCommandsController.Get(null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        private LoRaWANCommandsController CreateDeviceModelCommandsController()
        {
            return new LoRaWANCommandsController(
                this.mockLoRaWANCommandService.Object);
        }
    }
}
