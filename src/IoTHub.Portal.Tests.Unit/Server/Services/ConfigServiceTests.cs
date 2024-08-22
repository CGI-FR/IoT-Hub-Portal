// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class ConfigServiceTests
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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
            _ = await configsServices.RollOutDeviceModelConfiguration(modelId, desiredProperties);

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
            _ = await configsServices.RollOutDeviceModelConfiguration(deviceType, desirectProperties);

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
                .ThrowsAsync(new RequestFailedException("test"));

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
            _ = await configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
                .ThrowsAsync(new RequestFailedException("test"));

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
            _ = await configsServices.RollOutDeviceConfiguration(modelId, desiredProperties, configurationName, targetTags);

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
                .ThrowsAsync(new RequestFailedException("test"));

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
        public void WhenGetConfigIsNullGetConfigModuleListShouldThrowInternalServerErrorException()
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
        public void WhenPropertiesDesiredIsInWrongFormatGetConfigModuleListShouldThrowInvalidOperationException()
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

        [Test]
        public async Task GetModelSystemModuleReturnList()
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
            var result = await configService.GetModelSystemModule(configTest.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetConfigIsNullGetModelSystemModuleShouldThrowInternalServerErrorException()
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
            var result = async () => await configService.GetModelSystemModule(configTest.Id);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenPropertiesDesiredIsInWrongFormatConfigIsNullGetModelSystemModuleShouldThrowInvalidOperationException()
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
            var result = async () => await configService.GetModelSystemModule(configTest.Id);

            // Assert
            _ = result.Should().ThrowAsync<InvalidOperationException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetConfigRouteListShouldReturnAList()
        {
            // Arrange
            var configService = CreateConfigsServices();

            var configTest = new Configuration(Guid.NewGuid().ToString());
            var listConfig = new List<Configuration>()
            {
                configTest
            };

            var edgeHubPropertiesDesired = new EdgeHubPropertiesDesired();
            var routes = new Dictionary<string, object>()
            {
                {"Route1", new object() },
                {"Route2", new object() }
            };

            edgeHubPropertiesDesired.Routes = routes;

            var mockConfigEnumerator = this.mockRepository.Create<IEnumerable<Configuration>>();

            configTest.Content.ModulesContent = new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    "$edgeHub", new Dictionary<string, object>()
                    {
                        {
                            "properties.desired", JObject.Parse(JsonConvert.SerializeObject(edgeHubPropertiesDesired))
                        }
                    }
                }
            };

            _ = mockConfigEnumerator.Setup(x => x.GetEnumerator()).Returns(listConfig.GetEnumerator);

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(mockConfigEnumerator.Object);

            // Act
            var result = await configService.GetConfigRouteList(configTest.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenGetConfigIsNullGetConfigRouteListShouldThrowInternalServerErrorException()
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
            var result = async () => await configService.GetConfigRouteList(configTest.Id);

            // Assert
            _ = result.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenPropertiesDesiredIsInWrongFormatGetConfigRouteListShouldThrowInvalidOperationException()
        {
            // Arrange
            var configService = CreateConfigsServices();

            var configTest = new Configuration(Guid.NewGuid().ToString());
            var listConfig = new List<Configuration>()
            {
                configTest
            };

            var edgeHubPropertiesDesired = new EdgeHubPropertiesDesired();
            var routes = new Dictionary<string, object>()
            {
                {"Route1", new object() },
                {"Route2", new object() }
            };

            edgeHubPropertiesDesired.Routes = routes;

            var mockConfigEnumerator = this.mockRepository.Create<IEnumerable<Configuration>>();

            configTest.Content.ModulesContent = new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    "$edgeHub", new Dictionary<string, object>()
                    {
                        {
                            "properties.desired", edgeHubPropertiesDesired
                        }
                    }
                }
            };

            _ = mockConfigEnumerator.Setup(x => x.GetEnumerator()).Returns(listConfig.GetEnumerator);

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(mockConfigEnumerator.Object);

            // Act
            var result = async () => await configService.GetConfigRouteList(configTest.Id);

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
            _ = await configsServices.RollOutEdgeModelConfiguration(edgeModel);

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
            _ = await configsServices.RollOutEdgeModelConfiguration(edgeModel);

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

        [Test]
        public async Task WhenConfigurationExistsDeleteDeviceModelConfigurationByConfigurationNamePrefixShouldRemoveIt()
        {
            // Arrange
            var configsServices = CreateConfigsServices();
            var configurationPrefix = Guid.NewGuid().ToString();
            var suffix = Guid.NewGuid().ToString();

            _ = this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[]
                {
                    new Configuration($"{configurationPrefix}-{suffix}"),
                    new Configuration($"null-{suffix}")
                });

            _ = this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == $"{configurationPrefix}-{suffix}")))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.DeleteDeviceModelConfigurationByConfigurationNamePrefix(configurationPrefix);

            // Assert
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task GetPublicEdgeModules_AzureContext_EmptyListIsReturned()
        {
            // Arrange
            var configsServices = CreateConfigsServices();

            //Arrange
            var publicEdgeModules = await configsServices.GetPublicEdgeModules();

            //Assert
            _ = publicEdgeModules.Should().BeEquivalentTo(Array.Empty<IoTEdgeModule>());
            this.mockRepository.VerifyAll();
        }
    }
}
