// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using Portal.Server.Controllers.v1._0;
    using Shared.Models.v1._0;

    [TestFixture]
    public class DeviceConfigurationsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceConfigurationsService> mockDeviceConfigurationsService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceConfigurationsService = this.mockRepository.Create<IDeviceConfigurationsService>();
        }

        private DeviceConfigurationsController CreateDeviceConfigurationsController()
        {
            return new DeviceConfigurationsController(this.mockDeviceConfigurationsService.Object);
        }

        [Test]
        public async Task GetShouldReturnDeviceConfigurationList()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.GetDeviceConfigurationListAsync())
                .ReturnsAsync(new List<ConfigListItem>()
                {
                    new ConfigListItem()
                });

            // Act
            var result = await deviceConfigController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetWithIdShouldReturnDeviceConfiguration()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            var configId = Guid.NewGuid().ToString();

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.GetDeviceConfigurationAsync(It.Is<string>(c => c.Equals(configId, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig()
                {
                    ConfigurationId = configId,
                    ModelId = Guid.NewGuid().ToString(),
                });

            // Act
            var response = await deviceConfigController.Get(configId);

            // Assert
            Assert.IsNotNull(response);

            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okObjectResult)
            {
                Assert.IsNotNull(okObjectResult.Value);
                Assert.IsAssignableFrom<DeviceConfig>(okObjectResult.Value);
            }
            else
            {
                Assert.Fail("Cannot inspect the result.");
            }

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigurationMetricsShouldReturnMetrics()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            var configId = Guid.NewGuid().ToString();

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.GetConfigurationMetricsAsync(It.Is<string>(c => c.Equals(configId, StringComparison.Ordinal))))
                .ReturnsAsync(new ConfigurationMetrics() { CreationDate = DateTime.Now });

            // Act
            var result = await deviceConfigController.GetConfigurationMetrics(configId);

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceConfigurationShoulCreateConfiguration()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig()
            {
                ConfigurationId= Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.CreateConfigurationAsync(It.Is<DeviceConfig>(c => c.ConfigurationId.Equals(deviceConfig.ConfigurationId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigController.CreateConfig(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDeviceConfigurationShouldUpdateConfiguration()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig()
            {
                ConfigurationId= Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.UpdateConfigurationAsync(It.Is<DeviceConfig>(c => c.ConfigurationId.Equals(deviceConfig.ConfigurationId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigController.UpdateConfig(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceConfigurationShouldDeleteConfiguration()
        {
            // Arrange
            var deviceConfigController = CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig()
            {
                ConfigurationId= Guid.NewGuid().ToString(),
            };

            _ = this.mockDeviceConfigurationsService
                .Setup(x => x.DeleteConfigurationAsync(It.Is<string>(c => c.Equals(deviceConfig.ConfigurationId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigController.DeleteConfig(deviceConfig.ConfigurationId);

            // Assert
            this.mockRepository.VerifyAll();
        }
    }
}
