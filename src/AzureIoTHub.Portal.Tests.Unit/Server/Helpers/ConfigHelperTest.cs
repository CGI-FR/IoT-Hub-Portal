// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Helpers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigHelperTest
    {
        [Test]
        public void CreateDeviceConfigShouldReturnValue()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var targetCondition = $"tags.modelId = '{modelId}' and tags.name = 'test' and tags.name01 = 'test'";
            var desiredProperties = new Dictionary<string, object>()
            {
                {"properties.desired.test", "test"}
            };

            var config = new Configuration("test")
            {
                TargetCondition = targetCondition,
                Labels = new Dictionary<string, string> { { "id", "test" } },
                Priority = 1,
            };
            config.Content.DeviceContent = desiredProperties;

            // Act
            var result = ConfigHelper.CreateDeviceConfig(config);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Tags.Count > 0);
            Assert.IsNotNull(result.Properties);
            Assert.AreEqual(config.Priority, result.Priority);
            Assert.AreEqual(modelId, result.ModelId);
        }

        [Test]
        public void CreateDeviceConfigWithNullArgumentShouldThrowException()
        {
            // Arrange

            // Act

            // Assert
            var argumentNullException = Assert.Throws<ArgumentNullException>(() => ConfigHelper.CreateDeviceConfig(config: null));
            Assert.IsNotNull(argumentNullException);
            Assert.AreEqual("config", argumentNullException.ParamName);
            Assert.IsInstanceOf<ArgumentNullException>(argumentNullException);
        }

        [Test]
        public void CreateDeviceConfigWithNullTargetConditionShouldShouldThrowAnException()
        {
            // Arrange
            var config = new Configuration("test")
            {
                TargetCondition = string.Empty,
                Labels = new Dictionary<string, string> { { "id", "test" } },
                Priority = 1
            };

            // Act

            // Assert
            var invalidOperationException =  Assert.Throws<InvalidOperationException>(() => ConfigHelper.CreateDeviceConfig(config));
            Assert.IsNotNull(invalidOperationException);
            Assert.IsInstanceOf<InvalidOperationException>(invalidOperationException);
        }

        [Test]
        public void CreateGatewayModuleShouldReturnValue()
        {
            // Arrange
            var config = new Configuration("config_test");

            var jPropModule = new Dictionary<string, object>()
            {
                { "status", "running" },
                { "version", "1.0" },
                { "settings", new Dictionary<string, object>()
                    {
                        { "image", "image_test" }
                    }
                },
                { "env", new Dictionary<string, object>()
                    {
                        {
                            "envVariable", new Dictionary<string, object>()
                            {
                                { "value", "test" }
                            }
                        }
                    }
                }
            };

            var module = new JProperty("moduleTest", JObject.Parse(JsonConvert.SerializeObject(jPropModule)));

            // Act
            var result = ConfigHelper.CreateGatewayModule(config, module);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("running", result.Status);
            Assert.AreEqual("1.0", result.Version);
            Assert.AreEqual("image_test", result.ImageURI);
            Assert.AreEqual(1, result.EnvironmentVariables.Count);
        }

        [Test]
        public void CreateGatewayModuleWithNullConfigShouldThrowException()
        {
            // Arrange
            JProperty module = new("");

            // Act
            var result = () => ConfigHelper.CreateGatewayModule(null, module);

            // Assert
            _ = result.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateGatewayModuleWithNullModuleShouldThrowException()
        {
            // Arrange
            var config = new Configuration("config_test");

            // Act
            var result = () => ConfigHelper.CreateGatewayModule(config, null);

            // Assert
            _ = result.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GenerateModulesContentShouldReturnValue()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "Model test",
                Description = "Description Test",
                EdgeModules = new List<IoTEdgeModule>()
                {
                    new IoTEdgeModule()
                    {
                        ModuleName = "ModuleTest",
                        Status = "running",
                        Version = "1.0",
                        ImageURI = "image",
                        EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                        {
                            new IoTEdgeModuleEnvironmentVariable(){ Name = "envTest01", Value = "test" }
                        },
                        ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>()
                    }
                }
            };

            // Act
            var result = ConfigHelper.GenerateModulesContent(edgeModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<Dictionary<string, IDictionary<string, object>>>(result);
            Assert.AreEqual(2, result.Count);
        }
    }
}
