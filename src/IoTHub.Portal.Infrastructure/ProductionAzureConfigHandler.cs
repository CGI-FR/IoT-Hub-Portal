// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    public class ProductionAzureConfigHandler : ConfigHandlerBase
    {
        private readonly IConfiguration config;

        public ProductionAzureConfigHandler(IConfiguration config)
        {
            this.config = config;
        }

        public override string PortalName => this.config[PortalNameKey]!;

        public override int SyncDatabaseJobRefreshIntervalInMinutes => this.config.GetValue(SyncDatabaseJobRefreshIntervalKey, 5);

        public override int MetricExporterRefreshIntervalInSeconds => this.config.GetValue(MetricExporterRefreshIntervalKey, 30);

        public override int MetricLoaderRefreshIntervalInMinutes => this.config.GetValue(MetricLoaderRefreshIntervalKey, 10);

        public override string AzureIoTHubConnectionString => this.config.GetConnectionString(AzureIoTHubConnectionStringKey)!;

        public override string AzureIoTHubEventHubEndpoint => this.config.GetConnectionString(AzureIoTHubEventHubEndpointKey)!;

        public override string AzureIoTHubEventHubConsumerGroup => this.config.GetValue(AzureIoTHubEventHubConsumerGroupKey, "iothub-portal")!;

        public override string AzureDPSConnectionString => this.config.GetConnectionString(AzureDpsConnectionStringKey)!;

        public override string AzureDPSEndpoint => this.config[AzureDpsServiceEndpointKey]!;

        public override string AzureDPSScopeID => this.config[AzureDpsIdScopeKey]!;

        public override string AzureStorageAccountConnectionString => this.config.GetConnectionString(AzureStorageAccountConnectionStringKey)!;

        public override string PostgreSQLConnectionString => this.config.GetConnectionString(PostgreSqlConnectionStringKey)!;

        public override string MySQLConnectionString => this.config.GetConnectionString(MySqlConnectionStringKey)!;

        public override string DbProvider => this.config.GetValue(DbProviderKey, DbProviders.PostgreSQL)!;

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

        public override string AzureLoRaKeyManagementCode => this.config.GetConnectionString(AzureLoRaKeyManagementCodeKey)!;

        public override string AzureLoRaKeyManagementApiVersion => this.config[AzureLoRaKeyManagementApiVersionKey]!;

        public override bool IdeasEnabled => this.config.GetValue(IdeasEnabledKey, false);
        public override string IdeasUrl => this.config.GetValue(IdeasUrlKey, string.Empty)!;
        public override string IdeasAuthenticationHeader => this.config.GetValue(IdeasAuthenticationHeaderKey, "Ocp-Apim-Subscription-Key")!;
        public override string IdeasAuthenticationToken => this.config.GetValue(IdeasAuthenticationTokenKey, string.Empty)!;

        public override int SendCommandsToDevicesIntervalInMinutes => this.config.GetValue(SendCommandsToDevicesIntervalKey, 10);

        public override string CloudProvider => this.config[CloudProviderKey]!;

        public override string AWSAccess => throw new NotImplementedException();

        public override string AWSAccessSecret => throw new NotImplementedException();

        public override string AWSRegion => throw new NotImplementedException();
        public override string AWSS3StorageConnectionString => throw new NotImplementedException();

        public override string AWSBucketName => throw new NotImplementedException();
        public override string AWSAccountId => throw new NotImplementedException();

        public override IEnumerable<string> AWSGreengrassRequiredRoles => throw new NotImplementedException();

        public override string AWSGreengrassCoreTokenExchangeRoleAliasName => throw new NotImplementedException();
    }
}
