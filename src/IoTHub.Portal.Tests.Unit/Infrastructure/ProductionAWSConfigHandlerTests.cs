// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class ProductionAWSConfigHandlerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        public ProductionAWSConfigHandler CreateProductionAWSConfigHandler()
        {
            return new ProductionAWSConfigHandler(this.mockConfiguration.Object);
        }

        [TestCase(ConfigHandlerBase.PortalNameKey, nameof(ConfigHandlerBase.PortalName))]
        [TestCase(ConfigHandlerBase.AzureDPSServiceEndpointKey, nameof(ConfigHandlerBase.AzureDPSEndpoint))]
        [TestCase(ConfigHandlerBase.AzureDPSIDScopeKey, nameof(ConfigHandlerBase.AzureDPSScopeID))]
        [TestCase(ConfigHandlerBase.OIDCScopeKey, nameof(ConfigHandlerBase.OIDCScope))]
        [TestCase(ConfigHandlerBase.OIDCAuthorityKey, nameof(ConfigHandlerBase.OIDCAuthority))]
        [TestCase(ConfigHandlerBase.OIDCMetadataUrlKey, nameof(ConfigHandlerBase.OIDCMetadataUrl))]
        [TestCase(ConfigHandlerBase.OIDCClientIdKey, nameof(ConfigHandlerBase.OIDCClientId))]
        [TestCase(ConfigHandlerBase.OIDCApiClientIdKey, nameof(ConfigHandlerBase.OIDCApiClientId))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementUrlKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementUrl))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementCodeKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementCode))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementApiVersionKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementApiVersion))]
        [TestCase(ConfigHandlerBase.AzureIoTHubConnectionStringKey, nameof(ConfigHandlerBase.AzureIoTHubConnectionString))]
        [TestCase(ConfigHandlerBase.AzureDPSConnectionStringKey, nameof(ConfigHandlerBase.AzureDPSConnectionString))]
        [TestCase(ConfigHandlerBase.PostgreSQLConnectionStringKey, nameof(ConfigHandlerBase.PostgreSQLConnectionString))]
        [TestCase(ConfigHandlerBase.MySQLConnectionStringKey, nameof(ConfigHandlerBase.MySQLConnectionString))]
        [TestCase(ConfigHandlerBase.AWSAccessKey, nameof(ConfigHandlerBase.AWSAccess))]
        [TestCase(ConfigHandlerBase.AWSAccessSecretKey, nameof(ConfigHandlerBase.AWSAccessSecret))]
        [TestCase(ConfigHandlerBase.AWSRegionKey, nameof(ConfigHandlerBase.AWSRegion))]
        [TestCase(ConfigHandlerBase.AWSS3StorageConnectionStringKey, nameof(ConfigHandlerBase.AWSS3StorageConnectionString))]
        [TestCase(ConfigHandlerBase.CloudProviderKey, nameof(ConfigHandlerBase.CloudProvider))]
        [TestCase(ConfigHandlerBase.AWSBucketNameKey, nameof(ConfigHandlerBase.AWSBucketName))]
        [TestCase(ConfigHandlerBase.AWSAccountIdKey, nameof(ConfigHandlerBase.AWSAccountId))]
        [TestCase(ConfigHandlerBase.AWSGreengrassCoreTokenExchangeRoleAliasNameKey, nameof(ConfigHandlerBase.AWSGreengrassCoreTokenExchangeRoleAliasName))]
        public void SettingsShouldGetValueFromAppSettings(string configKey, string configPropertyName)
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();
            var configHandler = CreateProductionAWSConfigHandler();

            _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
                .Returns(expected);

            // Act
            var result = configHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(configHandler, null);

            // Assert
            Assert.AreEqual(expected, result);
            this.mockRepository.VerifyAll();
        }

        [TestCase(ConfigHandlerBase.AWSGreengrassRequiredRolesKey, nameof(ConfigHandlerBase.AWSGreengrassRequiredRoles))]
        public void SettingsShouldGetSectionFromAppSettings(string configKey, string configPropertyName)
        {
            // Arrange
            var mockSection = this.mockRepository.Create<IConfigurationSection>();

            _ = mockSection.SetupGet(c => c.Value)
                    .Returns(Guid.NewGuid().ToString());

            _ = mockSection.SetupGet(c => c.Path)
                .Returns(configKey);

            _ = mockSection.Setup(c => c.GetChildren())
                .Returns(Array.Empty<IConfigurationSection>());

            var configHandler = CreateProductionAWSConfigHandler();

            _ = this.mockConfiguration.Setup(c => c.GetSection(It.Is<string>(x => x == configKey)))
                .Returns(mockSection.Object);

            // Act
            var result = configHandler
                                .GetType()
                                .GetProperty(configPropertyName)
                                .GetValue(configHandler, null);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [TestCase(nameof(ConfigHandlerBase.AzureStorageAccountConnectionString))]
        [TestCase(nameof(ConfigHandlerBase.AzureDPSConnectionString))]
        [TestCase(nameof(ConfigHandlerBase.AzureDPSEndpoint))]
        [TestCase(nameof(ConfigHandlerBase.AzureDPSScopeID))]
        [TestCase(nameof(ConfigHandlerBase.AzureIoTHubConnectionString))]
        [TestCase(nameof(ConfigHandlerBase.AzureIoTHubEventHubConsumerGroup))]
        [TestCase(nameof(ConfigHandlerBase.AzureIoTHubEventHubEndpoint))]
        //[TestCase(nameof(ConfigHandlerBase.AzureIsLoRaEnabled))]
        [TestCase(nameof(ConfigHandlerBase.AzureLoRaKeyManagementApiVersion))]
        [TestCase(nameof(ConfigHandlerBase.AzureLoRaKeyManagementCode))]
        [TestCase(nameof(ConfigHandlerBase.AzureLoRaKeyManagementUrl))]
        [TestCase(nameof(ConfigHandlerBase.StorageAccountDeviceModelImageMaxAge))]
        public void SettingsShouldThrowError(string configPropertyName)
        {
            // Arrange
            var productionConfigHandler = CreateProductionAWSConfigHandler();

            // Act
            var result = () => productionConfigHandler.GetType().GetProperty(configPropertyName).GetValue(productionConfigHandler, null);

            // Assert
            _ = result.Should().Throw<TargetInvocationException>();

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
            var configHandler = CreateProductionAWSConfigHandler();
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
            var configHandler = CreateProductionAWSConfigHandler();
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

        //[TestCase(ConfigHandlerBase.AzureIsLoRaFeatureEnabledKey, nameof(ConfigHandlerBase.AzureIsLoRaEnabled))]
        //public void SettingsShouldGetBoolFromAppSettings(string configKey, string configPropertyName)
        //{
        //    // Arrange
        //    var expected = false;
        //    var productionConfigHandler = CreateProductionAWSConfigHandler();

        //    _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
        //        .Returns(Convert.ToString(expected, CultureInfo.InvariantCulture));

        //    // Act
        //    var result = productionConfigHandler
        //                        .GetType()
        //                        .GetProperty(configPropertyName)
        //                        .GetValue(productionConfigHandler, null);

        //    // Assert
        //    Assert.AreEqual(expected, result);

        //    //// Arrange
        //    expected = true;

        //    _ = this.mockConfiguration.SetupGet(c => c[It.Is<string>(x => x == configKey)])
        //        .Returns(Convert.ToString(expected, CultureInfo.InvariantCulture));

        //    // Act
        //    result = productionConfigHandler
        //                        .GetType()
        //                        .GetProperty(configPropertyName)
        //                        .GetValue(productionConfigHandler, null);

        //    // Assert
        //    Assert.AreEqual(expected, result);
        //    this.mockRepository.VerifyAll();
        //}

        [Test]
        public void SyncDatabaseJobRefreshIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.SyncDatabaseJobRefreshIntervalInMinutes.Should().Be(5);
        }

        [Test]
        public void MetricExporterRefreshIntervalInSecondsConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.MetricExporterRefreshIntervalInSeconds.Should().Be(30);
        }

        [Test]
        public void MetricLoaderRefreshIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.MetricLoaderRefreshIntervalInMinutes.Should().Be(10);
        }

        [Test]
        public void IdeasEnabledMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.IdeasEnabled.Should().BeFalse();
        }

        [Test]
        public void IdeasUrlMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.IdeasUrl.Should().BeEmpty();
        }

        [Test]
        public void IdeasAuthenticationHeaderMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.IdeasAuthenticationHeader.Should().Be("Ocp-Apim-Subscription-Key");
        }

        [Test]
        public void IdeasAuthenticationTokenMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.IdeasAuthenticationToken.Should().BeEmpty();
        }

        [Test]
        public void IoTHubEventHubEndpoint_GetDefaultValue_ReturnsEmpty()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.AzureIoTHubEventHubEndpoint.Should().BeEmpty();
        }

        [Test]
        public void IoTHubEventHubConsumerGroup_GetDefaultValue_ReturnsExpectedDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.AzureIoTHubEventHubConsumerGroup.Should().Be("iothub-portal");
        }

        [Test]
        public void DbProviderKeyShouldBeExpectedDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.DbProvider.Should().Be(DbProviders.PostgreSQL);
        }

        [Test]
        public void SendCommandsToDevicesIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var productionAWSConfigHandler = new ProductionAWSConfigHandler(new ConfigurationManager());

            // Assert
            _ = productionAWSConfigHandler.SendCommandsToDevicesIntervalInMinutes.Should().Be(10);
        }
    }
}
