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

        public override string AzureDpsConnectionString => this.config[AzureDpsConnectionStringKey]!;

        public override string AzureDpsEndpoint => this.config[AzureDpsServiceEndpointKey]!;

        public override string AzureDpsScopeId => this.config[AzureDpsIdScopeKey]!;

        public override string AzureStorageAccountConnectionString => this.config[AzureStorageAccountConnectionStringKey]!;

        public override int StorageAccountDeviceModelImageMaxAge => this.config.GetValue(StorageAccountDeviceModelImageMaxAgeKey, 86400);

        public override bool UseSecurityHeaders => this.config.GetValue(UseSecurityHeadersKey, true);

        public override string OidcScope => this.config[OidcScopeKey]!;

        public override string OidcAuthority => this.config[OidcAuthorityKey]!;

        public override string OidcMetadataUrl => this.config[OidcMetadataUrlKey]!;

        public override string OidcClientId => this.config[OidcClientIdKey]!;

        public override string OidcApiClientId => this.config[OidcApiClientIdKey]!;

        public override bool OidcValidateIssuer => this.config.GetValue(OidcValidateIssuerKey, true);

        public override bool OidcValidateAudience => this.config.GetValue(OidcValidateAudienceKey, true);

        public override bool OidcValidateLifetime => this.config.GetValue(OidcValidateLifetimeKey, true);

        public override bool OidcValidateIssuerSigningKey => this.config.GetValue(OidcValidateIssuerSigningKeyKey, true);

        public override bool OidcValidateActor => this.config.GetValue(OidcValidateActorKey, false);

        public override bool OidcValidateTokenReplay => this.config.GetValue(OidcValidateTokenReplayKey, false);

        public override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "false");

        public override string AzureLoRaKeyManagementUrl => this.config[AzureLoRaKeyManagementUrlKey]!;

        public override string AzureLoRaKeyManagementCode => this.config[AzureLoRaKeyManagementCodeKey]!;

        public override string AzureLoRaKeyManagementApiVersion => this.config[AzureLoRaKeyManagementApiVersionKey]!;

        public override bool IdeasEnabled => this.config.GetValue(IdeasEnabledKey, false);
        public override string IdeasUrl => this.config.GetValue(IdeasUrlKey, string.Empty)!;
        public override string IdeasAuthenticationHeader => this.config.GetValue(IdeasAuthenticationHeaderKey, "Ocp-Apim-Subscription-Key")!;
        public override string IdeasAuthenticationToken => this.config.GetValue(IdeasAuthenticationTokenKey, string.Empty)!;

        public override string PostgreSqlConnectionString => this.config[PostgreSqlConnectionStringKey]!;

        public override string MySqlConnectionString => this.config[MySqlConnectionStringKey]!;

        public override string DbProvider => this.config.GetValue(DbProviderKey, DbProviders.PostgreSql)!;

        public override string CloudProvider => this.config[CloudProviderKey]!;

        public override string AwsAccess => this.config[AwsAccessKey]!;
        public override string AwsAccessSecret => this.config[AwsAccessSecretKey]!;
        public override string AwsRegion => this.config[AwsRegionKey]!;
        public override string Awss3StorageConnectionString => this.config[Awss3StorageConnectionStringKey]!;
        public override string AwsBucketName => this.config[AwsBucketNameKey]!;
        public override string AwsAccountId => this.config[AwsAccountIdKey]!;
        public override IEnumerable<string> AwsGreengrassRequiredRoles => this.config.GetSection(AwsGreengrassRequiredRolesKey).Get<string[]>()!;
        public override string AwsGreengrassCoreTokenExchangeRoleAliasName => this.config[AwsGreengrassCoreTokenExchangeRoleAliasNameKey]!;

        public override int SendCommandsToDevicesIntervalInMinutes => this.config.GetValue(SendCommandsToDevicesIntervalKey, 10);
    }
}
