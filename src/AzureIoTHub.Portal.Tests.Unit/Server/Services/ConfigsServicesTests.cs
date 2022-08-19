// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class ConfigsServicesTests
    {
        private MockRepository mockRepository;

        private Mock<RegistryManager> mockRegistryManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
        }

        private ConfigService CreateConfigsServices()
        {
            return new ConfigService(this.mockRegistryManager.Object);
        }

        [Test]
        public async Task GetIoTEdgeConfigsShouldReturnModulesConfigurations()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");

            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(new[]
                {
                    new Configuration("aaa"),
                    iotEdgeConfiguration
                });

            // Act
            var result = await configsServices.GetIoTEdgeConfigurations();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("bbb", result.Single().Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetIoTEdgeConfigurationsShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");

            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.GetIoTEdgeConfigurations();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesConfigsShouldReturnDeviceTwinConfigurations()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");

            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(new[]
                {
                    new Configuration("aaa") { Priority = 100 },
                    iotEdgeConfiguration
                });

            // Act
            var result = await configsServices.GetDevicesConfigurations();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("aaa", result.Single().Id);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevicesConfigsShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");

            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.GetDevicesConfigurations();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            const string id = "aaa";
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationAsync(It.Is<string>(x => x == id)))
                .ReturnsAsync(new Configuration(id));

            // Act
            var result = await configsServices.GetConfigItem(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigItemShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            const string id = "aaa";
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationAsync(It.Is<string>(x => x == id)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.GetConfigItem(id);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task RolloutDeviceModelConfigurationStateUnderTestExpectedBehavior(string modelId, string configurationPrefix)
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            Configuration newConfiguration = null;
            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(Array.Empty<Configuration>());

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            // Act
            await configsServices.RollOutDeviceModelConfiguration(modelId, desiredProperties);

            // Assert
            Assert.IsNotNull(newConfiguration);
            Assert.IsTrue(newConfiguration.Id.StartsWith(configurationPrefix, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, newConfiguration.Content.ModulesContent.Count);
            Assert.AreEqual(0, newConfiguration.Content.ModuleContent.Count);
            Assert.AreEqual(1, newConfiguration.Content.DeviceContent.Count);
            Assert.AreEqual(desiredProperties, newConfiguration.Content.DeviceContent);
            Assert.AreEqual($"tags.modelId = '{modelId}'", newConfiguration.TargetCondition);
            Assert.AreEqual(0, newConfiguration.Priority);

            this.mockRepository.VerifyAll();
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task WhenConfigurationExistsRolloutDeviceModelConfigurationShouldRemoveIt(string deviceType, string configurationPrefix)
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desirectProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            Configuration newConfiguration;
            var suffix = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[]
                {
                    new Configuration($"{configurationPrefix}-{suffix}"),
                    new Configuration($"null-{suffix}")
                });

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == $"{configurationPrefix}-{suffix}")))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.RollOutDeviceModelConfiguration(deviceType, desirectProperties);

            // Assert
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task DeleteConfigurationShouldDeleteToRegistry()
        {
            // Arrange
            var configsServices = CreateConfigsServices();

            var configurationId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == configurationId)))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.DeleteConfiguration(configurationId);

            // Assert
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == configurationId)), Times.Once());
        }

        [Test]
        public async Task DeleteConfigurationShouldThrowInternalServerErrorExceptionWhenIssueOccurs()
        {
            // Arrange
            var configsServices = CreateConfigsServices();

            var configurationId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == configurationId)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.DeleteConfiguration(configurationId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [TestCase]
        public async Task RolloutDeviceConfigurationStateUnderTestExpectedBehavior()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            var targetTags = new Dictionary<string, string>
            {
                { "tag1", "tagValue1" }
            };
            var configurationName = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            Configuration newConfiguration = null;

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(Array.Empty<Configuration>());

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationName))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            // Act
            await configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

            // Assert
            Assert.IsNotNull(newConfiguration);
            Assert.IsTrue(newConfiguration.Id.StartsWith(configurationName, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(0, newConfiguration.Content.ModulesContent.Count);
            Assert.AreEqual(0, newConfiguration.Content.ModuleContent.Count);
            Assert.AreEqual(1, newConfiguration.Content.DeviceContent.Count);
            Assert.AreEqual(desiredProperties, newConfiguration.Content.DeviceContent);
            Assert.AreEqual($"tags.modelId = '{modelId}' and tags.tag1 = 'tagValue1'", newConfiguration.TargetCondition);
            Assert.AreEqual(0, newConfiguration.Priority);

            this.mockRepository.VerifyAll();
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public async Task RolloutDeviceConfigurationShouldThrowInternalServerErrorExceptionWhenIssueOccursOnGettingConfiguration()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            var targetTags = new Dictionary<string, string>
            {
                { "tag1", "tagValue1" }
            };
            var configurationName = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task RolloutDeviceConfigurationShouldThrowInternalServerErrorExceptionWhenIssueOccursOnRemovingConfiguration()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            var targetTags = new Dictionary<string, string>
            {
                { "tag1", "tagValue1" }
            };
            var configurationName = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(new[] { new Configuration(configurationName) });

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task RolloutDeviceConfigurationShouldThrowInternalServerErrorExceptionWhenIssueOccursOnAddingConfiguration()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            var targetTags = new Dictionary<string, string>
            {
                { "tag1", "tagValue1" }
            };
            var configurationName = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(Array.Empty<Configuration>());

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationName))))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenConfigurationExistsRolloutDeviceConfigurationShouldRemoveIt()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            var targetTags = new Dictionary<string, string>
            {
                { "tag1", "tagValue1" }
            };
            var configurationName = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            Configuration newConfiguration;
            var suffix = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[]
                {
                    new Configuration($"{configurationName}-{suffix}"),
                    new Configuration($"null-{suffix}")
                });

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationName))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == $"{configurationName}-{suffix}")))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

            // Assert
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task GetFailedDeploymentsCountShouldReturnFailedDeploymentsCount()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");
            iotEdgeConfiguration.SystemMetrics.Results.Add("reportedFailedCount", 2);
            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(new[]
                {
                    iotEdgeConfiguration
                });

            // Act
            var result = await configsServices.GetFailedDeploymentsCount();

            // Assert
            _ = result.Should().Be(2);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetFailedDeploymentsCountShouldInternalServerErrorExceptionWhenIssueOccursOnGettingFailedDeploymentsCount()
        {
            // Arrange
            var configsServices = CreateConfigsServices();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ThrowsAsync(new Exception("test"));

            // Act
            var act = () => configsServices.GetFailedDeploymentsCount();

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigModuleListShouldReturnAList()
        {
            // Arrange
            var configService = CreateConfigsServices();

            var configTest = new Configuration(Guid.NewGuid().ToString());
            var listConfig = new List<Configuration>()
            {
                configTest
            };

            var edgeAgentPropertiesDesired = new EdgeAgentPropertiesDesired();
            var modules = new Dictionary<string, ConfigModule>()
            {
                {"module test 01", new ConfigModule() },
                {"module test 02", new ConfigModule() }
            };

            edgeAgentPropertiesDesired.Modules = modules;

            var mockConfigEnumerator = this.mockRepository.Create<IEnumerable<Configuration>>();

            configTest.Content.ModulesContent = new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    "$edgeAgent", new Dictionary<string, object>()
                    {
                        {
                            "properties.desired", JObject.Parse(JsonConvert.SerializeObject(edgeAgentPropertiesDesired))
                        }
                    }
                }
            };

            _ = mockConfigEnumerator.Setup(x => x.GetEnumerator()).Returns(listConfig.GetEnumerator);

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(mockConfigEnumerator.Object);

            // Act
            var result = await configService.GetConfigModuleList(configTest.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetConfigIsNullConfigModuleListShouldReturnAList()
        {
            // Arrange
            var configService = CreateConfigsServices();

            var configTest = new Configuration(Guid.NewGuid().ToString());
            var listConfig = new List<Configuration>()
            {
            };

            var mockConfigEnumerator = this.mockRepository.Create<IEnumerable<Configuration>>();

            _ = mockConfigEnumerator.Setup(x => x.GetEnumerator()).Returns(listConfig.GetEnumerator);

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(mockConfigEnumerator.Object);

            // Act
            var result = async () => await configService.GetConfigModuleList(configTest.Id);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenPropertiesDesiredIsInWrongFormatGetConfigModuleListShouldReturnAList()
        {
            // Arrange
            var configService = CreateConfigsServices();

            var configTest = new Configuration(Guid.NewGuid().ToString());
            var listConfig = new List<Configuration>()
            {
                configTest
            };

            var edgeAgentPropertiesDesired = new EdgeAgentPropertiesDesired();
            var modules = new Dictionary<string, ConfigModule>()
            {
                {"module test 01", new ConfigModule() },
                {"module test 02", new ConfigModule() }
            };

            edgeAgentPropertiesDesired.Modules = modules;

            var mockConfigEnumerator = this.mockRepository.Create<IEnumerable<Configuration>>();

            configTest.Content.ModulesContent = new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    "$edgeAgent", new Dictionary<string, object>()
                    {
                        {
                            "properties.desired", edgeAgentPropertiesDesired
                        }
                    }
                }
            };

            _ = mockConfigEnumerator.Setup(x => x.GetEnumerator()).Returns(listConfig.GetEnumerator);

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(mockConfigEnumerator.Object);

            // Act
            var result = async () => await configService.GetConfigModuleList(configTest.Id);

            // Assert
            _ = result.Should().ThrowAsync<InvalidOperationException>();

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task RollOutEdgeModelConfigurationShouldCreateNewConfiguration(string modelId, string configurationPrefix)
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = modelId
            };

            Configuration newConfiguration = null;

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(Array.Empty<Configuration>());

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            // Act
            await configsServices.RollOutEdgeModelConfiguration(edgeModel);

            // Assert
            Assert.IsNotNull(newConfiguration);
            Assert.IsTrue(newConfiguration.Id.StartsWith(configurationPrefix, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(2, newConfiguration.Content.ModulesContent.Count);
            Assert.AreEqual(0, newConfiguration.Content.ModuleContent.Count);
            Assert.AreEqual(0, newConfiguration.Content.DeviceContent.Count);
            Assert.AreEqual($"tags.modelId = '{modelId}'", newConfiguration.TargetCondition);
            Assert.AreEqual(10, newConfiguration.Priority);

            this.mockRepository.VerifyAll();
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task WhenConfigurationExistsRollOutEdgeModelConfigurationShouldRemoveIt(string modelId, string configurationPrefix)
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = modelId
            };
            Configuration newConfiguration;
            var suffix = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[]
                {
                    new Configuration($"{configurationPrefix}-{suffix}"),
                    new Configuration($"null-{suffix}")
                });

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == $"{configurationPrefix}-{suffix}")))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.RollOutEdgeModelConfiguration(edgeModel);

            // Assert
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.IsAny<string>()), Times.Once());
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public void WhenAddGonfigurationFailedRollOutEdgeModelConfigurationShouldThrowInternalServerErrorException(string modelId, string configurationPrefix)
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = modelId
            };

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(Array.Empty<Configuration>());

            _ = this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .ThrowsAsync(new Exception(""));

            // Act
            var result = async () => await configsServices.RollOutEdgeModelConfiguration(edgeModel);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
        }
    }
}
