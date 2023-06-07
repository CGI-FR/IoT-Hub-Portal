// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    using IoTHub.Portal.Domain;

    internal abstract class ConfigHandlerBase : ConfigHandler
    {
        internal const string PortalNameKey = "PortalName";
        internal const string AzureIoTHubConnectionStringKey = "Azure:IoTHub:ConnectionString";
        internal const string AzureIoTHubEventHubEndpointKey = "Azure:IoTHub:EventHub:Endpoint";
        internal const string AzureIoTHubEventHubConsumerGroupKey = "Azure:IoTHub:EventHub:ConsumerGroup";
        internal const string AzureDPSConnectionStringKey = "Azure:IoTDPS:ConnectionString";
        internal const string AzureDPSServiceEndpointKey = "Azure:IoTDPS:ServiceEndpoint";
        internal const string AzureDPSIDScopeKey = "Azure:IoTDPS:IDScope";
        internal const string UseSecurityHeadersKey = "UseSecurityHeaders";
        internal const string PostgreSQLConnectionStringKey = "PostgreSQL:ConnectionString";
        internal const string MySQLConnectionStringKey = "MySQL:ConnectionString";
        internal const string DbProviderKey = "DbProvider";

        internal const string OIDCScopeKey = "OIDC:Scope";
        internal const string OIDCAuthorityKey = "OIDC:Authority";
        internal const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
        internal const string OIDCClientIdKey = "OIDC:ClientId";
        internal const string OIDCApiClientIdKey = "OIDC:ApiClientId";
        internal const string OIDCValidateIssuerKey = "OIDC:ValidateIssuer";
        internal const string OIDCValidateAudienceKey = "OIDC:ValidateAudience";
        internal const string OIDCValidateLifetimeKey = "OIDC:ValidateLifetime";
        internal const string OIDCValidateIssuerSigningKeyKey = "OIDC:ValidateIssuerSigningKey";
        internal const string OIDCValidateActorKey = "OIDC:ValidateActor";
        internal const string OIDCValidateTokenReplayKey = "OIDC:ValidateTokenReplay";

        internal const string AzureIsLoRaFeatureEnabledKey = "Azure:LoRaFeature:Enabled";

        internal const string AzureStorageAccountConnectionStringKey = "Azure:StorageAccount:ConnectionString";
        internal const string StorageAccountDeviceModelImageMaxAgeKey = "StorageAccount:DeviceModel:Image:MaxAgeInSeconds";

        internal const string AzureLoRaKeyManagementUrlKey = "Azure:LoRaKeyManagement:Url";
        internal const string AzureLoRaKeyManagementCodeKey = "Azure:LoRaKeyManagement:Code";
        internal const string AzureLoRaKeyManagementApiVersionKey = "Azure:LoRaKeyManagement:ApiVersion";

        internal const string MetricExporterRefreshIntervalKey = "Metrics:ExporterRefreshIntervalInSeconds";
        internal const string MetricLoaderRefreshIntervalKey = "Metrics:LoaderRefreshIntervalInMinutes";

        internal const string SyncDatabaseJobRefreshIntervalKey = "Job:SyncDatabaseJobRefreshIntervalInMinutes";

        internal const string IdeasEnabledKey = "Ideas:Enabled";
        internal const string IdeasUrlKey = "Ideas:Url";
        internal const string IdeasAuthenticationHeaderKey = "Ideas:Authentication:Header";
        internal const string IdeasAuthenticationTokenKey = "Ideas:Authentication:Token";

        internal const string CloudProviderKey = "CloudProvider";

        internal const string AWSAccessKey = "AWS:Access";
        internal const string AWSAccessSecretKey = "AWS:AccessSecret";
        internal const string AWSRegionKey = "AWS:Region";
        internal const string AWSS3StorageConnectionStringKey = "AWS:S3Storage:ConnectionString";
        internal const string AWSBucketNameKey = "AWS:BucketName";
        internal const string AWSAccountIdKey = "AWS:AccountId";
        internal const string AWSGreengrassRequiredRolesKey = "AWS:GreengrassRequiredRoles";
        internal const string AWSGreengrassCoreTokenExchangeRoleAliasNameKey = "AWS:GreengrassCoreTokenExchangeRoleAliasName";
    }
}
