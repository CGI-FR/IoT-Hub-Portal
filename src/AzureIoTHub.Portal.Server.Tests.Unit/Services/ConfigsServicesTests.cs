using AzureIoTHub.Portal.Server.Services;
using Microsoft.Azure.Devices;
using Moq;
using NUnit.Framework;
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
    }
}
