using AzureIoTHub.Portal.Server.Controllers.V10;
using AzureIoTHub.Portal.Server.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using static AzureIoTHub.Portal.Server.Startup;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class SettingsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<IOptions<ClientApiIndentityOptions>> mockConfiguration;
        private Mock<ConfigHandler> mockConfigHandler;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IOptions<ClientApiIndentityOptions>>();
            this.mockConfigHandler = this.mockRepository.Create<ConfigHandler>();
        }

        private SettingsController CreateController()
        {
            return new SettingsController(this.mockConfiguration.Object, this.mockConfigHandler.Object);
        }

        [Test]
        public void GetOIDCSettings_Should_return_value()
        {
            // Arrange
            var clientApiIndentityOptions = new ClientApiIndentityOptions
            {
                Authority = Guid.NewGuid().ToString(),
                ClientId = Guid.NewGuid().ToString(),
                MetadataUrl = Guid.NewGuid().ToString(),
                Scope = Guid.NewGuid().ToString(),
            };

            this.mockConfiguration
                .SetupGet(c => c.Value)
                .Returns(value: clientApiIndentityOptions);

            var controller = CreateController();

            // Act
            var response = controller.GetOIDCSettings();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response);
            var okObjectResult = response as ObjectResult;

            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.AreEqual(clientApiIndentityOptions, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }
        
        [Test]
        public void GetLoRaActivationSetting_Should_return_true_to_string()
        {
            // Arrange
            bool loraFeatureStatus = true;

            this.mockConfiguration.SetupGet(c => c.Value).Returns(value: null);

            this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(loraFeatureStatus);

            var controller = this.CreateController();

            // Act
            var response = controller.GetLoRaActivationSetting();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response);
            var okObjectResult = response as ObjectResult;

            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.AreEqual(loraFeatureStatus, okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }
    }
}
