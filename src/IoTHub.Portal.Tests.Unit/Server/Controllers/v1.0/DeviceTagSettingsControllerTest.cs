// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Controllers.V10;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceTagSettingsControllerTest
    {
        private MockRepository mockRepository;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<ILogger<DeviceTagSettingsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceTagSettingsController>>();
        }

        private DeviceTagSettingsController CreateDeviceTagSettingsController()
        {
            return new DeviceTagSettingsController(
            this.mockLogger.Object,
            //this.mockDeviceTagMapper.Object,
            //this.mockTableClientFactory.Object,
            this.mockDeviceTagService.Object
           );
        }

        [Test]
        public async Task PostShouldCreateNewEntity()
        {
            // Arrange
            var deviceTagSettingsController = CreateDeviceTagSettingsController();

            var tag = new DeviceTagDto
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            _ = this.mockDeviceTagService.Setup(c => c.UpdateTags(It.IsAny<List<DeviceTagDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceTagSettingsController.Post(new List<DeviceTagDto>(new[] { tag }));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockDeviceTagService.VerifyAll();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetShouldReturnAList()
        {
            // Arrange
            var deviceTagSettingsController = CreateDeviceTagSettingsController();

            _ = this.mockDeviceTagService.Setup(x => x.GetAllTags()).Returns(new DeviceTagDto[10].ToList());

            // Act
            var response = deviceTagSettingsController.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse?.StatusCode);

            Assert.IsNotNull(okResponse?.Value);
            var result = okResponse?.Value as IEnumerable<DeviceTagDto>;
            Assert.AreEqual(10, result?.Count());

            this.mockDeviceTagService.VerifyAll();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateOrUpdateDeviceTagShouldCreateOrUpdateDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString(),
            };

            var deviceTagSettingsController = CreateDeviceTagSettingsController();

            _ = this.mockDeviceTagService.Setup(x => x.CreateOrUpdateDeviceTag(It.Is<DeviceTagDto>(x => x.Name.Equals(deviceTag.Name, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            var response = await deviceTagSettingsController.CreateOrUpdateDeviceTag(deviceTag);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkResult>(response);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceTagByNameShouldDeleteDeviceTag()
        {
            // Arrange
            var deviceTag = new DeviceTagDto
            {
                Name = Guid.NewGuid().ToString(),
            };

            var deviceTagSettingsController = CreateDeviceTagSettingsController();

            _ = this.mockDeviceTagService.Setup(x => x.DeleteDeviceTagByName(It.Is<string>(x => x.Equals(deviceTag.Name, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            var response = await deviceTagSettingsController.DeleteDeviceTagByName(deviceTag.Name);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkResult>(response);
            this.mockRepository.VerifyAll();
        }
    }
}
