// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    public class DevelopmentConfigHandler : ConfigHandlerBase
    {
        private readonly IConfiguration config;

        public DevelopmentConfigHandler(IConfiguration config)
        {
            this.config = config;
        }

        public override string PortalName => this.config[PortalNameKey]!;

        public override int SyncDatabaseJobRefreshIntervalInMinutes => this.config.GetValue(SyncDatabaseJobRefreshIntervalKey, 5);

        public override int MetricExporterRefreshIntervalInSeconds => this.config.GetValue(MetricExporterRefreshIntervalKey, 30);

        public override int MetricLoaderRefreshIntervalInMinutes => this.config.GetValue(MetricLoaderRefreshIntervalKey, 10);

        public override string AzureIoTHubConnectionString => this.config[AzureIoTHubConnectionStringKey]!;

        public override string AzureIoTHubEventHubEndpoint => this.config.GetValue(AzureIoTHubEventHubEndpointKey, string.Empty)!;

        public override string AzureIoTHubEventHubConsumerGroup => this.config.GetValue(AzureIoTHubEventHubConsumerGroupKey, "iothub-portal")!;

        public override string AzureDPSConnectionString => this.config[AzureDpsConnectionStringKey]!;

        public override string AzureDPSEndpoint => this.config[AzureDpsServiceEndpointKey]!;

        public override string AzureDPSScopeID => this.config[AzureDpsIdScopeKey]!;

        public override string AzureStorageAccountConnectionString => this.config[AzureStorageAccountConnectionStringKey]!;

        public override int StorageAccountDeviceModelImageMaxAge => this.config.GetValue(StorageAccountDeviceModelImageMaxAgeKey, 86400);

        public override bool UseSecurityHeaders => this.config.GetValue(UseSecurityHeadersKey, true);

        public override string OIDCScope => this.config[OidcScopeKey]!;

        public override string OIDCAuthority => this.config[OidcAuthorityKey]!;

        public override string OIDCMetadataUrl => this.config[OidcMetadataUrlKey]!;

        public override string OIDCClientId => this.config[OidcClientIdKey]!;

        public override string OIDCApiClientId => this.config[OidcApiClientIdKey]!;

        public override bool OIDCValidateIssuer => this.config.GetValue(OidcValidateIssuerKey, true);

        public override bool OIDCValidateAudience => this.config.GetValue(OidcValidateAudienceKey, true);

        public override bool OIDCValidateLifetime => this.config.GetValue(OidcValidateLifetimeKey, true);

        public override bool OIDCValidateIssuerSigningKey => this.config.GetValue(OidcValidateIssuerSigningKeyKey, true);

        public override bool OIDCValidateActor => this.config.GetValue(OidcValidateActorKey, false);

        public override bool OIDCValidateTokenReplay => this.config.GetValue(OidcValidateTokenReplayKey, false);

        public override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "false");

        public override string AzureLoRaKeyManagementUrl => this.config[AzureLoRaKeyManagementUrlKey]!;

        public override string AzureLoRaKeyManagementCode => this.config[AzureLoRaKeyManagementCodeKey]!;

        public override string AzureLoRaKeyManagementApiVersion => this.config[AzureLoRaKeyManagementApiVersionKey]!;

        public override bool IdeasEnabled => this.config.GetValue(IdeasEnabledKey, false);
        public override string IdeasUrl => this.config.GetValue(IdeasUrlKey, string.Empty)!;
        public override string IdeasAuthenticationHeader => this.config.GetValue(IdeasAuthenticationHeaderKey, "Ocp-Apim-Subscription-Key")!;
        public override string IdeasAuthenticationToken => this.config.GetValue(IdeasAuthenticationTokenKey, string.Empty)!;

        public override string PostgreSQLConnectionString => this.config[PostgreSqlConnectionStringKey]!;

        public override string MySQLConnectionString => this.config[MySqlConnectionStringKey]!;

        public override string DbProvider => this.config.GetValue(DbProviderKey, DbProviders.PostgreSQL)!;

        public override string CloudProvider => this.config[CloudProviderKey]!;

        public override string AWSAccess => this.config[AwsAccessKey]!;
        public override string AWSAccessSecret => this.config[AwsAccessSecretKey]!;
        public override string AWSRegion => this.config[AwsRegionKey]!;
        public override string AWSS3StorageConnectionString => this.config[Awss3StorageConnectionStringKey]!;
        public override string AWSBucketName => this.config[AwsBucketNameKey]!;
        public override string AWSAccountId => this.config[AwsAccountIdKey]!;
        public override IEnumerable<string> AWSGreengrassRequiredRoles => this.config.GetSection(AwsGreengrassRequiredRolesKey).Get<string[]>()!;
        public override string AWSGreengrassCoreTokenExchangeRoleAliasName => this.config[AwsGreengrassCoreTokenExchangeRoleAliasNameKey]!;

        public override int SendCommandsToDevicesIntervalInMinutes => this.config.GetValue(SendCommandsToDevicesIntervalKey, 10);
    }
}
