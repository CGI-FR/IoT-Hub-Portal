using AzureIoTHub.Portal.Server.Services;
using Microsoft.Azure.Devices;
using Moq;
using NUnit.Framework;
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

        private ConfigsServices CreateConfigsServices()
        {
            return new ConfigsServices(this.mockRegistryManager.Object);
        }

        [Test]
        public async Task GetAllConfigs_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var configsServices = this.CreateConfigsServices();
            this.mockRegistryManager.Setup(c => c.GetConfigurationsAsync(It.Is<int>(x => x == 0)))
                .ReturnsAsync(new[]
                {
                    new Configuration("aaa"),
                    new Configuration("bbb")
                });

            // Act
            var result = await configsServices.GetAllConfigs();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("aaa", result.First().Id);
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
