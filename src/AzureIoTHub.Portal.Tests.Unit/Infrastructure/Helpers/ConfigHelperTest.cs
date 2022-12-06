// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzureIoTHub.Portal.Application.Helpers;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.IoTEdgeModule;
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

            var expectedContainerCreateOptions = /*lang=json,strict*/ "{\"HostConfig\":{\"PortBindings\":{\"5671/tcp\":[{\"HostPort\":\"5671\"}],\"8883/tcp\":[{\"HostPort\":\"8883\"}],\"443/tcp\":[{\"HostPort\":\"443\"}]}}}";

            var jPropModule = new Dictionary<string, object>()
            {
                { "status", "running" },
                { "version", "1.0" },
                { "settings", new Dictionary<string, object>()
                    {
                        { "image", "image_test" },
                        { "createOptions", expectedContainerCreateOptions }
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
            Assert.AreEqual("image_test", result.ImageURI);
            Assert.AreEqual(expectedContainerCreateOptions, result.ContainerCreateOptions);
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
            var expectedModuleName = "ModuleTest";
            var expectedContainerCreateOptions = /*lang=json,strict*/
                "{\"HostConfig\":{\"PortBindings\":{\"5671/tcp\":[{\"HostPort\":\"5671\"}],\"8883/tcp\":[{\"HostPort\":\"8883\"}],\"443/tcp\":[{\"HostPort\":\"443\"}]}}}";

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "Model test",
                Description = "Description Test",
                EdgeModules = new List<IoTEdgeModule>()
                {
                    new()
                    {
                        ModuleName = expectedModuleName,
                        Status = "running",
                        ImageURI = "image",
                        ContainerCreateOptions = expectedContainerCreateOptions,
                        EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                        {
                            new() { Name = "envTest01", Value = "test" }
                        },
                        ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>()
                    }
                },
                SystemModules = new List<EdgeModelSystemModule>()
                {
                    new EdgeModelSystemModule("edgeAgent")
                    {
                        ImageUri = "image",
                        ContainerCreateOptions = Guid.NewGuid().ToString(),
                        EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                        {
                            new IoTEdgeModuleEnvironmentVariable(){ Name ="test", Value = "test" }
                        }
                    },
                    new EdgeModelSystemModule("edgeHub")
                    {
                        ImageUri = "image",
                        ContainerCreateOptions = Guid.NewGuid().ToString(),
                        EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                        {
                            new IoTEdgeModuleEnvironmentVariable(){ Name ="test", Value = "test" }
                        }
                    }
                }
            };

            // Act
            var result = ConfigHelper.GenerateModulesContent(edgeModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<Dictionary<string, IDictionary<string, object>>>(result);
            Assert.AreEqual(2, result.Count);
            var edgeHubPropertiesDesired = (EdgeAgentPropertiesDesired)result["$edgeAgent"]["properties.desired"];
            _ = edgeHubPropertiesDesired.Modules[expectedModuleName].Settings.CreateOptions.Should()
                .Be(expectedContainerCreateOptions);
        }

        [Test]
        public void CreateModuleTwinSettingsShouldReturnAList()
        {
            // Arrange
            var moduleName = "moduleTest";
            var modulesContent = new Dictionary<string, IDictionary<string, object>>()
            {
                { "$edgeAgent", new Dictionary<string, object>() },
                { "$edgeHub", new Dictionary<string, object>() },
                {
                    moduleName, new Dictionary<string, object>()
                    {
                        { "properties.desired.setting01","test" },
                        { "properties.desired.setting02","test02" },
                    }
                },
            };

            // Act
            var result = ConfigHelper.CreateModuleTwinSettings(modulesContent, moduleName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("setting01", result[0].Name);
        }

        [Test]
        public void GenerateRoutesContentShouldReturnValue()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "Model test",
                Description = "Description Test",
                EdgeRoutes = new List<IoTEdgeRoute>()
                {
                    new IoTEdgeRoute()
                    {
                        Name = "RouteName",
                        Value = "FROM source INTO sink",
                        Priority = 5,
                        TimeToLive = 7200
                    }
                }
            };

            // Act
            var result = ConfigHelper.GenerateRoutesContent(edgeModel.EdgeRoutes);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<EdgeHubPropertiesDesired>(result);

            var firstRoute = result.Routes.FirstOrDefault();
            Assert.AreEqual("RouteName", firstRoute.Key);
            Assert.AreEqual("{ route = FROM source INTO sink, priority = 5, timeToLiveSecs = 7200 }", firstRoute.Value.ToString());
        }

        [Test]
        public void GenerateRoutesContentWithNullRoutesShouldThrowException()
        {
            // Arrange+Act
            var result = () => ConfigHelper.GenerateRoutesContent(null);

            // Assert
            _ = result.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateIoTEdgeRouteFromJPropertyWithNullRouteShouldThrowException()
        {
            // Arrange+Act
            var result = () => ConfigHelper.CreateIoTEdgeRouteFromJProperty(null);

            // Assert
            _ = result.Should().Throw<ArgumentNullException>();
        }
    }
}
