// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using Entities;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Azure.Devices;
    using Models;
    using Models.v10;
    using Moq;
    using NUnit.Framework;
    using Server.Controllers.v10;
    using Server.Exceptions;
    using Server.Factories;
    using Server.Services;
    using Configuration = Microsoft.Azure.Devices.Configuration;

    [TestFixture]
    public class DeviceConfigurationsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfigService> mockConfigService;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceModelPropertiesTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigService = this.mockRepository.Create<IConfigService>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceModelPropertiesTableClient = this.mockRepository.Create<TableClient>();
        }

        private DeviceConfigurationsController CreateDeviceConfigurationsController()
        {
            return new DeviceConfigurationsController(
                this.mockConfigService.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task GetConfigurationsStateUnderTestExpectedBehavior()
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
        public async Task GetConfigurationStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
            var response = await deviceConfigurationsController.Get(configurationId);

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
        public async Task GetConfigurationShouldReturnProperlyTheProperties()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
            var response = await deviceConfigurationsController.Get(configurationId);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            if (response.Result is OkObjectResult okObjectResult)
            {
                Assert.IsNotNull(okObjectResult.Value);
                Assert.IsAssignableFrom<DeviceConfig>(okObjectResult.Value);
                var resultConfig = okObjectResult.Value as DeviceConfig;
                Assert.IsNotNull(resultConfig);

                Assert.AreEqual("toto", resultConfig.Properties["test"]);
            }
            else
            {
                Assert.Fail("Cannot inspect the result.");
            }

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
        public async Task CreateConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

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
                .Returns(Task.CompletedTask);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.CreateConfig(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateConfigShouldThrowInternalServerErrorExceptionOnGettingDeviceModelProperties()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{deviceConfig.ModelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            var act = () => deviceConfigurationsController.CreateConfig(deviceConfig);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

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
                .Returns(Task.CompletedTask);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.UpdateConfig(deviceConfig);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateConfigShouldThrowInternalServerErrorExceptionOnGettingDeviceModelProperties()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

            var deviceConfig = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{deviceConfig.ModelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            var act = () => deviceConfigurationsController.UpdateConfig(deviceConfig);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteConfigStateUnderTestExpectedBehavior()
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

        [Test]
        public async Task GetConfigurationMetricsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();

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
            var result = await deviceConfigurationsController.GetConfigurationMetrics(deviceConfig.ConfigurationId);

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
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
                .Returns(Task.CompletedTask)
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = propertyType
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.UpdateConfig(deviceConfig);

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
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
                .Returns(Task.CompletedTask)
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = propertyType
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.CreateConfig(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.AreEqual(expected, requestedProperties[$"properties.desired.{propertyName}"]);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenPropertyNotPresentInModelUpdateConfigShouldNotUpdateTheProperty()
        {
            // Arrange
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
                .Returns(Task.CompletedTask)
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = DevicePropertyType.String
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.UpdateConfig(deviceConfig);

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
            var deviceConfigurationsController = this.CreateDeviceConfigurationsController();
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
                .Returns(Task.CompletedTask)
                .Callback((string _, Dictionary<string, object> properties, string _,
                    Dictionary<string, string> _, int _) => requestedProperties = properties);

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.Query<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ deviceConfig.ModelId }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<DeviceModelProperty>.FromPages(new[]
                {
                    Page<DeviceModelProperty>.FromValues(new[]
                    {
                        new DeviceModelProperty
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = deviceConfig.ModelId,
                            Name = propertyName,
                            PropertyType = DevicePropertyType.String
                        }
                    }, null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            await deviceConfigurationsController.CreateConfig(deviceConfig);

            // Assert
            Assert.IsNotNull(requestedProperties);
            Assert.IsTrue(requestedProperties.Count == 1);
            Assert.IsTrue(requestedProperties.ContainsKey($"properties.desired.{propertyName}"));

            this.mockRepository.VerifyAll();
        }
    }
}
