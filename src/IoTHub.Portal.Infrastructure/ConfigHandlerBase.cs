// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    public abstract class ConfigHandlerBase : ConfigHandler
    {
        public const string PortalNameKey = "PortalName";
        public const string AzureIoTHubConnectionStringKey = "Azure:IoTHub:ConnectionString";
        public const string AzureIoTHubEventHubEndpointKey = "Azure:IoTHub:EventHub:Endpoint";
        public const string AzureIoTHubEventHubConsumerGroupKey = "Azure:IoTHub:EventHub:ConsumerGroup";
        public const string AzureDpsConnectionStringKey = "Azure:IoTDPS:ConnectionString";
        public const string AzureDpsServiceEndpointKey = "Azure:IoTDPS:ServiceEndpoint";
        public const string AzureDpsIdScopeKey = "Azure:IoTDPS:IDScope";
        public const string UseSecurityHeadersKey = "UseSecurityHeaders";
        public const string PostgreSqlConnectionStringKey = "PostgreSQL:ConnectionString";
        public const string MySqlConnectionStringKey = "MySQL:ConnectionString";
        public const string DbProviderKey = "DbProvider";

        public const string OidcScopeKey = "OIDC:Scope";
        public const string OidcAuthorityKey = "OIDC:Authority";
        public const string OidcMetadataUrlKey = "OIDC:MetadataUrl";
        public const string OidcClientIdKey = "OIDC:ClientId";
        public const string OidcApiClientIdKey = "OIDC:ApiClientId";
        public const string OidcValidateIssuerKey = "OIDC:ValidateIssuer";
        public const string OidcValidateAudienceKey = "OIDC:ValidateAudience";
        public const string OidcValidateLifetimeKey = "OIDC:ValidateLifetime";
        public const string OidcValidateIssuerSigningKeyKey = "OIDC:ValidateIssuerSigningKey";
        public const string OidcValidateActorKey = "OIDC:ValidateActor";
        public const string OidcValidateTokenReplayKey = "OIDC:ValidateTokenReplay";

        public const string IsLoRaFeatureEnabledKey = "LoRaFeature:Enabled";

        public const string AzureStorageAccountConnectionStringKey = "Azure:StorageAccount:ConnectionString";
        public const string StorageAccountDeviceModelImageMaxAgeKey = "StorageAccount:DeviceModel:Image:MaxAgeInSeconds";

        public const string AzureLoRaKeyManagementUrlKey = "Azure:LoRaKeyManagement:Url";
        public const string AzureLoRaKeyManagementCodeKey = "Azure:LoRaKeyManagement:Code";
        public const string AzureLoRaKeyManagementApiVersionKey = "Azure:LoRaKeyManagement:ApiVersion";

        public const string MetricExporterRefreshIntervalKey = "Metrics:ExporterRefreshIntervalInSeconds";
        public const string MetricLoaderRefreshIntervalKey = "Metrics:LoaderRefreshIntervalInMinutes";

        public const string SyncDatabaseJobRefreshIntervalKey = "Job:SyncDatabaseJobRefreshIntervalInMinutes";
        public const string SendCommandsToDevicesIntervalKey = "Job:SendCommandsToDevicesIntervalInMinutes";

        public const string IdeasEnabledKey = "Ideas:Enabled";
        public const string IdeasUrlKey = "Ideas:Url";
        public const string IdeasAuthenticationHeaderKey = "Ideas:Authentication:Header";
        public const string IdeasAuthenticationTokenKey = "Ideas:Authentication:Token";

        public const string CloudProviderKey = "CloudProvider";

        public const string AwsAccessKey = "AWS:Access";
        public const string AwsAccessSecretKey = "AWS:AccessSecret";
        public const string AwsRegionKey = "AWS:Region";
        public const string Awss3StorageConnectionStringKey = "AWS:S3Storage:ConnectionString";
        public const string AwsBucketNameKey = "AWS:BucketName";
        public const string AwsAccountIdKey = "AWS:AccountId";
        public const string AwsGreengrassRequiredRolesKey = "AWS:GreengrassRequiredRoles";
        public const string AwsGreengrassCoreTokenExchangeRoleAliasNameKey = "AWS:GreengrassCoreTokenExchangeRoleAliasName";
    }
}
