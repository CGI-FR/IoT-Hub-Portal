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
        public const string AzureDPSConnectionStringKey = "Azure:IoTDPS:ConnectionString";
        public const string AzureDPSServiceEndpointKey = "Azure:IoTDPS:ServiceEndpoint";
        public const string AzureDPSIDScopeKey = "Azure:IoTDPS:IDScope";
        public const string UseSecurityHeadersKey = "UseSecurityHeaders";
        public const string PostgreSQLConnectionStringKey = "PostgreSQL:ConnectionString";
        public const string MySQLConnectionStringKey = "MySQL:ConnectionString";
        public const string DbProviderKey = "DbProvider";

        public const string OIDCScopeKey = "OIDC:Scope";
        public const string OIDCAuthorityKey = "OIDC:Authority";
        public const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
        public const string OIDCClientIdKey = "OIDC:ClientId";
        public const string OIDCApiClientIdKey = "OIDC:ApiClientId";
        public const string OIDCValidateIssuerKey = "OIDC:ValidateIssuer";
        public const string OIDCValidateAudienceKey = "OIDC:ValidateAudience";
        public const string OIDCValidateLifetimeKey = "OIDC:ValidateLifetime";
        public const string OIDCValidateIssuerSigningKeyKey = "OIDC:ValidateIssuerSigningKey";
        public const string OIDCValidateActorKey = "OIDC:ValidateActor";
        public const string OIDCValidateTokenReplayKey = "OIDC:ValidateTokenReplay";

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

        public const string AWSAccessKey = "AWS:Access";
        public const string AWSAccessSecretKey = "AWS:AccessSecret";
        public const string AWSRegionKey = "AWS:Region";
        public const string AWSS3StorageConnectionStringKey = "AWS:S3Storage:ConnectionString";
        public const string AWSBucketNameKey = "AWS:BucketName";
        public const string AWSAccountIdKey = "AWS:AccountId";
        public const string AWSGreengrassRequiredRolesKey = "AWS:GreengrassRequiredRoles";
        public const string AWSGreengrassCoreTokenExchangeRoleAliasNameKey = "AWS:GreengrassCoreTokenExchangeRoleAliasName";
    }
}
