// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure
{
    using System;
    using System.Globalization;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Infrastructure;

    [TestFixture]
    public class ProductionConfigHandlerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private ProductionConfigHandler CreateProductionConfigHandler()
        {
            return new ProductionConfigHandler(this.mockConfiguration.Object);
        }

        [TestCase(ConfigHandlerBase.IoTHubConnectionStringKey, nameof(ConfigHandlerBase.IoTHubConnectionString))]
        [TestCase(ConfigHandlerBase.DPSConnectionStringKey, nameof(ConfigHandlerBase.DPSConnectionString))]
        [TestCase(ConfigHandlerBase.StorageAccountConnectionStringKey, nameof(ConfigHandlerBase.StorageAccountConnectionString))]
        [TestCase(ConfigHandlerBase.LoRaKeyManagementCodeKey, nameof(ConfigHandlerBase.LoRaKeyManagementCode))]
        [TestCase(ConfigHandlerBase.PostgreSQLConnectionStringKey, nameof(ConfigHandlerBase.PostgreSQLConnectionString))]
        [TestCase(ConfigHandlerBase.IoTHubEventHubEndpointKey, nameof(ConfigHandlerBase.IoTHubEventHubEndpoint))]
        public void SecretsShouldGetValueFromConnectionStrings(string configKey, string configPropertyName)
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();
            var productionConfigHandler = CreateProductionConfigHandler();
            var mockConfigurationSection = this.mockRepository.Create<IConfigurationSection>();

            _ = this.mockConfiguration.Setup(c => c.GetSection(It.Is<string>(x => x == "ConnectionStrings")))
                .Returns(mockConfigurationSection.Object);

            _ = mockConfigurationSection.SetupGet(c => c[It.Is<string>(x => x == configKey)])
                .Returns(expected);

            // Act
            var result = productionConfigHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(productionConfigHandler, null);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [TestCase(ConfigHandlerBase.PortalNameKey, nameof(ConfigHandlerBase.PortalName))]
        [TestCase(ConfigHandlerBase.DPSServiceEndpointKey, nameof(ConfigHandlerBase.DPSEndpoint))]
        [TestCase(ConfigHandlerBase.DPSIDScopeKey, nameof(ConfigHandlerBase.DPSScopeID))]
        [TestCase(ConfigHandlerBase.OIDCScopeKey, nameof(ConfigHandlerBase.OIDCScope))]
        [TestCase(ConfigHandlerBase.OIDCAuthorityKey, nameof(ConfigHandlerBase.OIDCAuthority))]
        [TestCase(ConfigHandlerBase.OIDCMetadataUrlKey, nameof(ConfigHandlerBase.OIDCMetadataUrl))]
        [TestCase(ConfigHandlerBase.OIDCClientIdKey, nameof(ConfigHandlerBase.OIDCClientId))]
        [TestCase(ConfigHandlerBase.OIDCApiClientIdKey, nameof(ConfigHandlerBase.OIDCApiClientId))]
        [TestCase(ConfigHandlerBase.LoRaKeyManagementUrlKey, nameof(ConfigHandlerBase.LoRaKeyManagementUrl))]
        [TestCase(ConfigHandlerBase.LoRaKeyManagementApiVersionKey, nameof(ConfigHandlerBase.LoRaKeyManagementApiVersion))]
        public void SettingsShouldGetValueFromAppSettings(string configKey, string configPropertyName)
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();
            var productionConfigHandler = CreateProductionConfigHandler();

            _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
                .Returns(expected);

            // Act
            var result = productionConfigHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(productionConfigHandler, null);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [TestCase(ConfigHandlerBase.OIDCValidateAudienceKey, nameof(ConfigHandlerBase.OIDCValidateAudience))]
        [TestCase(ConfigHandlerBase.OIDCValidateIssuerKey, nameof(ConfigHandlerBase.OIDCValidateIssuer))]
        [TestCase(ConfigHandlerBase.OIDCValidateIssuerSigningKeyKey, nameof(ConfigHandlerBase.OIDCValidateIssuerSigningKey))]
        [TestCase(ConfigHandlerBase.OIDCValidateLifetimeKey, nameof(ConfigHandlerBase.OIDCValidateLifetime))]
        [TestCase(ConfigHandlerBase.UseSecurityHeadersKey, nameof(ConfigHandlerBase.UseSecurityHeaders))]
        public void SecuritySwitchesShouldBeEnabledByDefault(string configKey, string configPropertyName)
        {
            // Arrange
            var configHandler = CreateProductionConfigHandler();
            var mockConfigurationSection = this.mockRepository.Create<IConfigurationSection>();

            _ = mockConfigurationSection.SetupGet(c => c.Value)
                .Returns((string)null);

            _ = mockConfigurationSection.SetupGet(c => c.Path)
                .Returns(string.Empty);

            _ = this.mockConfiguration.Setup(c => c.GetSection(configKey))
                .Returns(mockConfigurationSection.Object);

            // Act
            var result = (bool)configHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(configHandler, null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestCase(ConfigHandlerBase.OIDCValidateActorKey, nameof(ConfigHandlerBase.OIDCValidateActor))]
        [TestCase(ConfigHandlerBase.OIDCValidateTokenReplayKey, nameof(ConfigHandlerBase.OIDCValidateTokenReplay))]
        public void SecuritySwitchesShouldBeDisabledByDefault(string configKey, string configPropertyName)
        {
            // Arrange
            var configHandler = CreateProductionConfigHandler();
            var mockConfigurationSection = this.mockRepository.Create<IConfigurationSection>();

            _ = mockConfigurationSection.SetupGet(c => c.Value)
                .Returns((string)null);

            _ = mockConfigurationSection.SetupGet(c => c.Path)
                .Returns(string.Empty);

            _ = this.mockConfiguration.Setup(c => c.GetSection(configKey))
                .Returns(mockConfigurationSection.Object);

            // Act
            var result = (bool)configHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(configHandler, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestCase(ConfigHandlerBase.IsLoRaFeatureEnabledKey, nameof(ConfigHandlerBase.IsLoRaEnabled))]
        public void SettingsShouldGetBoolFromAppSettings(string configKey, string configPropertyName)
        {
            // Arrange
            var expected = false;
            var productionConfigHandler = CreateProductionConfigHandler();

            _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
                .Returns(Convert.ToString(expected, CultureInfo.InvariantCulture));

            // Act
            var result = productionConfigHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(productionConfigHandler, null);

            // Assert
            Assert.AreEqual(expected, result);

            // Arrange
            expected = true;

            _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
                .Returns(Convert.ToString(expected, CultureInfo.InvariantCulture));

            // Act
            result = productionConfigHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(productionConfigHandler, null);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void SyncDatabaseJobRefreshIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.SyncDatabaseJobRefreshIntervalInMinutes.Should().Be(5);
        }

        [Test]
        public void MetricExporterRefreshIntervalInSecondsConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.MetricExporterRefreshIntervalInSeconds.Should().Be(30);
        }

        [Test]
        public void MetricLoaderRefreshIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.MetricLoaderRefreshIntervalInMinutes.Should().Be(10);
        }

        [Test]
        public void IdeasEnabledMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.IdeasEnabled.Should().BeFalse();
        }

        [Test]
        public void IdeasUrlMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.IdeasUrl.Should().BeEmpty();
        }

        [Test]
        public void IdeasAuthenticationHeaderMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.IdeasAuthenticationHeader.Should().Be("Ocp-Apim-Subscription-Key");
        }

        [Test]
        public void IdeasAuthenticationTokenMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.IdeasAuthenticationToken.Should().BeEmpty();
        }

        [Test]
        public void StorageAccountDeviceModelImageMaxAgeMustHaveDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.StorageAccountDeviceModelImageMaxAge.Should().Be(86400);
        }

        [Test]
        public void IoTHubEventHubConsumerGroup_GetDefaultValue_ReturnsExpectedDefaultValue()
        {
            // Arrange
            var productionConfigHandler = new ProductionConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionConfigHandler.IoTHubEventHubConsumerGroup.Should().Be("iothub-portal");
        }
    }
}
