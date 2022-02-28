using AzureIoTHub.Portal.Server.Services;
using Microsoft.Azure.Devices;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Services
{
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
        public async Task GetAllConfigs_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var configsServices = this.CreateConfigsServices();
            var iotEdgeConfiguration = new Configuration("bbb");

            iotEdgeConfiguration.Content.ModulesContent.Add("test", new Dictionary<string, object>());
            this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
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
        public async Task GetConfigItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var configsServices = this.CreateConfigsServices();
            string id = "aaa";
            this.mockRegistryManager.Setup(c => c.GetConfigurationAsync(It.Is<string>(x => x == id)))
                .ReturnsAsync(new Configuration(id));

            // Act
            var result = await configsServices.GetConfigItem(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);  
            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task RolloutDeviceConfiguration_StateUnderTest_ExpectedBehavior(string deviceType, string configurationPrefix)
        {
            // Arrange
            var configsServices = this.CreateConfigsServices();
            var desiredProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            Configuration newConfiguration = null;

            this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[0]);

            this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            // Act
            await configsServices.RolloutDeviceConfiguration(deviceType, desiredProperties);

            // Assert
            Assert.IsNotNull(newConfiguration);
            Assert.IsTrue(newConfiguration.Id.StartsWith(configurationPrefix));
            Assert.AreEqual(0, newConfiguration.Content.ModulesContent.Count);
            Assert.AreEqual(0, newConfiguration.Content.ModuleContent.Count);
            Assert.AreEqual(1, newConfiguration.Content.DeviceContent.Count);
            Assert.AreEqual(desiredProperties, newConfiguration.Content.DeviceContent);
            Assert.AreEqual($"tags.deviceType = '{deviceType}'", newConfiguration.TargetCondition);
            Assert.AreEqual(0, newConfiguration.Priority);

            this.mockRepository.VerifyAll();
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());

        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task When_Configuration_Exists_RolloutDeviceConfiguration_Should_Remove_It(string deviceType, string configurationPrefix)
        {
            // Arrange
            var configsServices = this.CreateConfigsServices();
            var desirectProperties = new Dictionary<string, object>
            {
                { "prop1", "value1" }
            };
            Configuration newConfiguration;
            var suffix = Guid.NewGuid().ToString();

            this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Configuration[]
                {
                    new Configuration($"{configurationPrefix}-{suffix}"),
                    new Configuration($"null-{suffix}")
                });

            this.mockRegistryManager.Setup(c => c.AddConfigurationAsync(It.Is<Configuration>(x => x.Id.StartsWith(configurationPrefix))))
                .Callback((Configuration conf) => newConfiguration = conf)
                .ReturnsAsync((Configuration conf) => conf);

            this.mockRegistryManager.Setup(c => c.RemoveConfigurationAsync(It.Is<string>(x => x == $"{configurationPrefix}-{suffix}")))
                .Returns(Task.CompletedTask);

            // Act
            await configsServices.RolloutDeviceConfiguration(deviceType, desirectProperties);

            // Assert
            this.mockRegistryManager.Verify(c => c.GetConfigurationsAsync(It.IsAny<int>()), Times.Once());
            this.mockRegistryManager.Verify(c => c.RemoveConfigurationAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
