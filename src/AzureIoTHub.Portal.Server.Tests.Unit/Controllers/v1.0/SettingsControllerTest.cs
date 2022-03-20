// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

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
        public void GetOIDCSettingsShouldReturnValue()
        {
            // Arrange
            var clientApiIndentityOptions = new ClientApiIndentityOptions
            {
                Authority = Guid.NewGuid().ToString(),
                ClientId = Guid.NewGuid().ToString(),
                MetadataUrl = new Uri("http://fake.local"),
                Scope = Guid.NewGuid().ToString(),
            };

            _ = this.mockConfiguration
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
        public void GetLoRaActivationSettingShouldReturnTrueToString()
        {
            // Arrange
            const bool loraFeatureStatus = true;

            _ = this.mockConfiguration.SetupGet(c => c.Value).Returns(value: null);

            _ = this.mockConfigHandler
                .SetupGet(c => c.IsLoRaEnabled)
                .Returns(loraFeatureStatus);

            _ = this.mockConfigHandler
                .SetupGet(c => c.PortalName)
                .Returns(string.Empty);

            var controller = CreateController();

            // Act
            var response = controller.GetPortalSetting();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response);
            var okObjectResult = response as ObjectResult;

            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<PortalSettings>(okObjectResult.Value);
            var okSettings = okObjectResult.Value as PortalSettings;

            Assert.AreEqual(loraFeatureStatus, okSettings?.IsLoRaSupported);

            this.mockRepository.VerifyAll();
        }
    }
}
