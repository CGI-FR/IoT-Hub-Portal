// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class DevelopmentConfigHandlerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private DevelopmentConfigHandler CreateDevelopmentConfigHandler()
        {
            return new DevelopmentConfigHandler(this.mockConfiguration.Object);
        }

        [TestCase(ConfigHandlerBase.PortalNameKey, nameof(ConfigHandlerBase.PortalName))]
        [TestCase(ConfigHandlerBase.AzureDpsServiceEndpointKey, nameof(ConfigHandlerBase.AzureDPSEndpoint))]
        [TestCase(ConfigHandlerBase.AzureDpsIdScopeKey, nameof(ConfigHandlerBase.AzureDPSScopeID))]
        [TestCase(ConfigHandlerBase.OidcScopeKey, nameof(ConfigHandlerBase.OIDCScope))]
        [TestCase(ConfigHandlerBase.OidcAuthorityKey, nameof(ConfigHandlerBase.OIDCAuthority))]
        [TestCase(ConfigHandlerBase.OidcMetadataUrlKey, nameof(ConfigHandlerBase.OIDCMetadataUrl))]
        [TestCase(ConfigHandlerBase.OidcClientIdKey, nameof(ConfigHandlerBase.OIDCClientId))]
        [TestCase(ConfigHandlerBase.OidcApiClientIdKey, nameof(ConfigHandlerBase.OIDCApiClientId))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementUrlKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementUrl))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementCodeKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementCode))]
        [TestCase(ConfigHandlerBase.AzureLoRaKeyManagementApiVersionKey, nameof(ConfigHandlerBase.AzureLoRaKeyManagementApiVersion))]
        [TestCase(ConfigHandlerBase.AzureIoTHubConnectionStringKey, nameof(ConfigHandlerBase.AzureIoTHubConnectionString))]
        [TestCase(ConfigHandlerBase.AzureDpsConnectionStringKey, nameof(ConfigHandlerBase.AzureDPSConnectionString))]
        [TestCase(ConfigHandlerBase.AzureStorageAccountConnectionStringKey, nameof(ConfigHandlerBase.AzureStorageAccountConnectionString))]
        [TestCase(ConfigHandlerBase.PostgreSqlConnectionStringKey, nameof(ConfigHandlerBase.PostgreSQLConnectionString))]
        [TestCase(ConfigHandlerBase.MySqlConnectionStringKey, nameof(ConfigHandlerBase.MySQLConnectionString))]
        [TestCase(ConfigHandlerBase.AwsAccessKey, nameof(ConfigHandlerBase.AWSAccess))]
        [TestCase(ConfigHandlerBase.AwsAccessSecretKey, nameof(ConfigHandlerBase.AWSAccessSecret))]
        [TestCase(ConfigHandlerBase.AwsRegionKey, nameof(ConfigHandlerBase.AWSRegion))]
        [TestCase(ConfigHandlerBase.Awss3StorageConnectionStringKey, nameof(ConfigHandlerBase.AWSS3StorageConnectionString))]
        [TestCase(ConfigHandlerBase.CloudProviderKey, nameof(ConfigHandlerBase.CloudProvider))]
        [TestCase(ConfigHandlerBase.AwsBucketNameKey, nameof(ConfigHandlerBase.AWSBucketName))]
        [TestCase(ConfigHandlerBase.AwsAccountIdKey, nameof(ConfigHandlerBase.AWSAccountId))]
        [TestCase(ConfigHandlerBase.AwsGreengrassCoreTokenExchangeRoleAliasNameKey, nameof(ConfigHandlerBase.AWSGreengrassCoreTokenExchangeRoleAliasName))]
        public void SettingsShouldGetValueFromAppSettings(string configKey, string configPropertyName)
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();
            var configHandler = CreateDevelopmentConfigHandler();

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

        [TestCase(ConfigHandlerBase.AwsGreengrassRequiredRolesKey, nameof(ConfigHandlerBase.AWSGreengrassRequiredRoles))]
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

            var configHandler = CreateDevelopmentConfigHandler();

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

        [TestCase(ConfigHandlerBase.OidcValidateAudienceKey, nameof(ConfigHandlerBase.OIDCValidateAudience))]
        [TestCase(ConfigHandlerBase.OidcValidateIssuerKey, nameof(ConfigHandlerBase.OIDCValidateIssuer))]
        [TestCase(ConfigHandlerBase.OidcValidateIssuerSigningKeyKey, nameof(ConfigHandlerBase.OIDCValidateIssuerSigningKey))]
        [TestCase(ConfigHandlerBase.OidcValidateLifetimeKey, nameof(ConfigHandlerBase.OIDCValidateLifetime))]
        [TestCase(ConfigHandlerBase.UseSecurityHeadersKey, nameof(ConfigHandlerBase.UseSecurityHeaders))]
        public void SecuritySwitchesShouldBeEnabledByDefault(string configKey, string configPropertyName)
        {
            // Arrange
            var configHandler = CreateDevelopmentConfigHandler();
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

        [TestCase(ConfigHandlerBase.OidcValidateActorKey, nameof(ConfigHandlerBase.OIDCValidateActor))]
        [TestCase(ConfigHandlerBase.OidcValidateTokenReplayKey, nameof(ConfigHandlerBase.OIDCValidateTokenReplay))]
        public void SecuritySwitchesShouldBeDisabledByDefault(string configKey, string configPropertyName)
        {
            // Arrange
            var configHandler = CreateDevelopmentConfigHandler();
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
            var productionConfigHandler = CreateDevelopmentConfigHandler();

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
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.SyncDatabaseJobRefreshIntervalInMinutes.Should().Be(5);
        }

        [Test]
        public void MetricExporterRefreshIntervalInSecondsConfigMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.MetricExporterRefreshIntervalInSeconds.Should().Be(30);
        }

        [Test]
        public void MetricLoaderRefreshIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.MetricLoaderRefreshIntervalInMinutes.Should().Be(10);
        }

        [Test]
        public void IdeasEnabledMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.IdeasEnabled.Should().BeFalse();
        }

        [Test]
        public void IdeasUrlMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.IdeasUrl.Should().BeEmpty();
        }

        [Test]
        public void IdeasAuthenticationHeaderMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.IdeasAuthenticationHeader.Should().Be("Ocp-Apim-Subscription-Key");
        }

        [Test]
        public void IdeasAuthenticationTokenMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.IdeasAuthenticationToken.Should().BeEmpty();
        }

        [Test]
        public void StorageAccountDeviceModelImageMaxAgeMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.StorageAccountDeviceModelImageMaxAge.Should().Be(86400);
        }

        [Test]
        public void IoTHubEventHubEndpoint_GetDefaultValue_ReturnsEmpty()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.AzureIoTHubEventHubEndpoint.Should().BeEmpty();
        }

        [Test]
        public void IoTHubEventHubConsumerGroup_GetDefaultValue_ReturnsExpectedDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.AzureIoTHubEventHubConsumerGroup.Should().Be("iothub-portal");
        }

        [Test]
        public void DbProviderKeyShouldBeExpectedDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.DbProvider.Should().Be(DbProviders.PostgreSQL);
        }

        [Test]
        public void SendCommandsToDevicesIntervalInMinutesConfigMustHaveDefaultValue()
        {
            // Arrange
            var developmentConfigHandler = new DevelopmentConfigHandler(new ConfigurationManager());

            // Assert
            _ = developmentConfigHandler.SendCommandsToDevicesIntervalInMinutes.Should().Be(10);
        }
    }
}
