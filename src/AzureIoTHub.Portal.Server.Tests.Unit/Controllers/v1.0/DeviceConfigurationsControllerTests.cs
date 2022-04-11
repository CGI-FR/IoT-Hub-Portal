// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Models.v10;
    using Moq;
    using NUnit.Framework;
    using Server.Controllers.v10;
    using Server.Services;

    [TestFixture]
    public class DeviceConfigurationsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfigService> mockConfigService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigService = this.mockRepository.Create<IConfigService>();
        }

        private DeviceConfigurationsController CreateDeviceConfigurationsController()
        {
            return new DeviceConfigurationsController(
                this.mockConfigService.Object);
        }

        [Test]
        public async Task GetConfigurations_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
            _ = this.mockConfigService.Setup(c => c.GetDevicesConfigurations())
                .ReturnsAsync(new List<Configuration>
                {
                    new Configuration(Guid.NewGuid().ToString())
                });

            // Act
            var result = await deviceConfigurationsController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfiguration_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var targetCondition = $"tags.modelId = '{modelId}'";

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(configurationId))
                .ReturnsAsync(new Configuration(configurationId)
                {
                    TargetCondition = targetCondition
                });

            // Act
            var response = await deviceConfigurationsController.Get(configurationId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as OkObjectResult;

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<DeviceConfig>(okObjectResult.Value);
            this.mockRepository.VerifyAll();
        }

        [TestCase(null, "Target condition is null.")]
        [TestCase("", "Target condition is null.")]
        [TestCase("fake condition", "Target condition is not formed as expected.")]
        public async Task WhenTargetConditionMalFormedShouldReturnBadRequest(string targetCondition, string errorMessage)
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(configurationId))
                .ReturnsAsync(new Configuration(configurationId)
                {
                    TargetCondition = targetCondition
                });

            // Act
            var response = await deviceConfigurationsController.Get(configurationId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);
            var okObjectResult = response.Result as BadRequestObjectResult;

            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<string>(okObjectResult.Value);
            Assert.AreEqual(errorMessage, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateConfig_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationID = Guid.NewGuid().ToString(),
                model = new DeviceModel()
                {
                    ModelId = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockConfigService.Setup(c =>
                    c.RolloutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.model.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationID),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigurationsController.CreateConfig(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteConfig_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
            var deviceConfigId =  Guid.NewGuid().ToString();

            _ = this.mockConfigService.Setup(c =>
                    c.DeleteConfiguration(It.Is<string>(x => x == deviceConfigId)))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigurationsController.DeleteConfig(
                deviceConfigId);

            // Assert
            this.mockRepository.VerifyAll();
        }
    }
}
