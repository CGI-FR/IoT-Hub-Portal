// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Moq;
    using NUnit.Framework;
    using Shared.Models;
    using Shared.Models.v1._0;
    using Configuration = Microsoft.Azure.Devices.Configuration;
    using ConfigurationMetrics = Microsoft.Azure.Devices.ConfigurationMetrics;

    [TestFixture]
    public class DeviceConfigurationsServiceTest : BackendUnitTest
    {
        private MockRepository mockRepository;

        private Mock<IConfigService> mockConfigService;
        private Mock<IDeviceModelPropertiesService> mockDeviceModelPropertiesService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigService = this.mockRepository.Create<IConfigService>();
            this.mockDeviceModelPropertiesService = this.mockRepository.Create<IDeviceModelPropertiesService>();
        }

        private DeviceConfigurationsService CreateService()
        {
            return new DeviceConfigurationsService(this.mockConfigService.Object, this.mockDeviceModelPropertiesService.Object);
        }

        [Test]
        public async Task GetConfigurationsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            _ = this.mockConfigService.Setup(c => c.GetDevicesConfigurations())
                .ReturnsAsync(new List<Configuration>
                {
                    new Configuration(Guid.NewGuid().ToString())
                });

            // Act
            var result = await deviceConfigurationsService.GetDeviceConfigurationListAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigurationStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var targetCondition = $"tags.modelId = '{modelId}'";

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(configurationId))
                .ReturnsAsync(new Configuration(configurationId)
                {
                    TargetCondition = targetCondition,
                    Labels = new AttributeDictionary()
                    {
                        new("configuration-id", Guid.NewGuid().ToString())
                    }
                });

            // Act
            var response = await deviceConfigurationsService.GetDeviceConfigurationAsync(configurationId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(modelId, response.ModelId);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigurationShouldReturnProperlyTheProperties()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var targetCondition = $"tags.modelId = '{modelId}'";

            var configuration = new Configuration(configurationId)
            {
                TargetCondition = targetCondition,
                Labels = new AttributeDictionary()
                {
                    new("configuration-id", Guid.NewGuid().ToString())
                }
            };

            configuration.Content.DeviceContent.Add("properties.desired.test", "toto");

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(configurationId))
                .ReturnsAsync(configuration);

            // Act
            var response = await deviceConfigurationsService.GetDeviceConfigurationAsync(configurationId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual("toto", response.Properties["test"]);

            this.mockRepository.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("fake condition")]
        public void WhenTargetConditionMalFormedShouldThrowInternalServerErrorException(string targetCondition)
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var configurationId = Guid.NewGuid().ToString();

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(configurationId))
                .ReturnsAsync(new Configuration(configurationId)
                {
                    TargetCondition = targetCondition
                });

            // Act
            var response =async () => await deviceConfigurationsService.GetDeviceConfigurationAsync(configurationId);

            // Assert
            _ = response.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .ReturnsAsync(new[]
                    {
                        new DeviceModelProperty
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModelId = deviceConfig.ModelId
                        }
                    });

            // Act
            await deviceConfigurationsService.CreateConfigurationAsync(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateConfigShouldThrowInternalServerErrorExceptionOnGettingDeviceModelProperties()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .Throws(new RequestFailedException("test"));

            // Act
            var act = () => deviceConfigurationsService.CreateConfigurationAsync(deviceConfig);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()));

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .ReturnsAsync(new[]
                    {
                        new DeviceModelProperty
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModelId = deviceConfig.ModelId
                        }
                    });

            // Act
            await deviceConfigurationsService.UpdateConfigurationAsync(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateConfigShouldThrowInternalServerErrorExceptionOnGettingDeviceModelProperties()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .Throws(new RequestFailedException("test"));

            // Act
            var act = () => deviceConfigurationsService.UpdateConfigurationAsync(deviceConfig);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var deviceConfigId =  Guid.NewGuid().ToString();

            _ = this.mockConfigService.Setup(c =>
                    c.DeleteConfiguration(It.Is<string>(x => x == deviceConfigId)))
                .Returns(Task.CompletedTask);

            // Act
            await deviceConfigurationsService.DeleteConfigurationAsync(deviceConfigId);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigurationMetricsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            var targetCondition = $"tags.modelId = '{deviceConfig.ModelId}'";

            var configurationMetrics = new ConfigurationMetrics();

            configurationMetrics.Results.Add("targetedCount", Random.Shared.Next());
            configurationMetrics.Results.Add("appliedCount", Random.Shared.Next());
            configurationMetrics.Results.Add("reportedSuccessfulCount", Random.Shared.Next());
            configurationMetrics.Results.Add("reportedFailedCount", Random.Shared.Next());

            _ = this.mockConfigService.Setup(c => c.GetConfigItem(deviceConfig.ConfigurationId))
                .ReturnsAsync(new Configuration(deviceConfig.ConfigurationId)
                {
                    TargetCondition = targetCondition,
                    Labels = new AttributeDictionary()
                    {
                        new("configuration-id", Guid.NewGuid().ToString())
                    },
                    Metrics = configurationMetrics
                });

            // Act
            var result = await deviceConfigurationsService.GetConfigurationMetricsAsync(deviceConfig.ConfigurationId);

            // Assert
            Assert.IsNotNull(result);
            this.mockRepository.VerifyAll();
        }

        [TestCase(DevicePropertyType.Boolean, "true", true)]
        [TestCase(DevicePropertyType.Boolean, "True", true)]
        [TestCase(DevicePropertyType.Boolean, "false", false)]
        [TestCase(DevicePropertyType.Boolean, "False", false)]
        [TestCase(DevicePropertyType.Boolean, "other", null)]
        [TestCase(DevicePropertyType.Double, "100.2", 100.2)]
        [TestCase(DevicePropertyType.Float, "100.2", 100.2f)]
        [TestCase(DevicePropertyType.Integer, "100", 100)]
        [TestCase(DevicePropertyType.Integer, "100.2", null)]
        [TestCase(DevicePropertyType.Long, "100", 100)]
        [TestCase(DevicePropertyType.Long, "100.2", null)]
        public async Task UpdateConfigShouldUpdatePropertyInValueType(DevicePropertyType propertyType, string propertyValue, object expected)
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var propertyName = Guid.NewGuid().ToString();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            Dictionary<string, object> requestedProperties = null;

            deviceConfig.Properties.Add(propertyName, propertyValue);

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()))
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .ReturnsAsync(new[]
                    {
                        new DeviceModelProperty
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModelId = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = propertyType
                        }
                    });

            // Act
            await deviceConfigurationsService.UpdateConfigurationAsync(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.AreEqual(expected, requestedProperties[$"properties.desired.{propertyName}"]);

            this.mockRepository.VerifyAll();
        }

        [TestCase(DevicePropertyType.Boolean, "true", true)]
        [TestCase(DevicePropertyType.Boolean, "True", true)]
        [TestCase(DevicePropertyType.Boolean, "false", false)]
        [TestCase(DevicePropertyType.Boolean, "False", false)]
        [TestCase(DevicePropertyType.Boolean, "other", null)]
        [TestCase(DevicePropertyType.Double, "100.2", 100.2)]
        [TestCase(DevicePropertyType.Float, "100.2", 100.2f)]
        [TestCase(DevicePropertyType.Integer, "100", 100)]
        [TestCase(DevicePropertyType.Integer, "100.2", null)]
        [TestCase(DevicePropertyType.Long, "100", 100)]
        [TestCase(DevicePropertyType.Long, "100.2", null)]
        public async Task CreateConfigShouldUpdatePropertyInValueType(DevicePropertyType propertyType, string propertyValue, object expected)
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var propertyName = Guid.NewGuid().ToString();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            Dictionary<string, object> requestedProperties = null;

            deviceConfig.Properties.Add(propertyName, propertyValue);

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()))
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                    .ReturnsAsync(new[]
                        {
                                        new DeviceModelProperty
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            ModelId = deviceConfig.ModelId,
                                            Name = propertyName,
                                            PropertyType = propertyType
                                        }
                        });

            // Act
            await deviceConfigurationsService.CreateConfigurationAsync(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.AreEqual(expected, requestedProperties[$"properties.desired.{propertyName}"]);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenPropertyNotPresentInModelUpdateConfigShouldNotUpdateTheProperty()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var propertyName = Guid.NewGuid().ToString();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            Dictionary<string, object> requestedProperties = null;

            deviceConfig.Properties.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            deviceConfig.Properties.Add(propertyName, Guid.NewGuid().ToString());

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()))
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .ReturnsAsync(new[]
                    {
                        new DeviceModelProperty
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModelId = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = DevicePropertyType.String
                        }
                    });

            // Act
            await deviceConfigurationsService.UpdateConfigurationAsync(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.IsTrue(requestedProperties.Count == 1);
            Assert.IsTrue(requestedProperties.ContainsKey($"properties.desired.{propertyName}"));

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenPropertyNotPresentInModelCreateConfigShouldNotUpdateTheProperty()
        {
            // Arrange
            var deviceConfigurationsService = CreateService();
            var propertyName = Guid.NewGuid().ToString();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            Dictionary<string, object> requestedProperties = null;

            deviceConfig.Properties.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            deviceConfig.Properties.Add(propertyName, Guid.NewGuid().ToString());

            _ = this.mockConfigService.Setup(c =>
                    c.RollOutDeviceConfiguration(It.Is<string>(x => x == deviceConfig.ModelId),
                        It.IsAny<Dictionary<string, object>>(),
                        It.Is<string>(x => x == deviceConfig.ConfigurationId),
                        It.IsAny<Dictionary<string, string>>(),
                        It.Is<int>(x => x == 100)))
                .Returns(Task.FromResult(Fixture.Create<string>()))
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            _ = this.mockDeviceModelPropertiesService.Setup(c => c.GetModelProperties(deviceConfig.ModelId))
                .ReturnsAsync(new[]
                    {
                        new DeviceModelProperty
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModelId = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = DevicePropertyType.String
                        }
                    });

            // Act
            await deviceConfigurationsService.CreateConfigurationAsync(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.IsTrue(requestedProperties.Count == 1);
            Assert.IsTrue(requestedProperties.ContainsKey($"properties.desired.{propertyName}"));

            this.mockRepository.VerifyAll();
        }
    }
}
