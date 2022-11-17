// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using AzureIoTHub.Portal.Domain;

    internal abstract class ConfigHandlerBase : ConfigHandler
    {
        internal const string PortalNameKey = "PortalName";
        internal const string IoTHubConnectionStringKey = "IoTHub:ConnectionString";
        internal const string IoTHubEventHubEndpointKey = "IoTHub:EventHub:Endpoint";
        internal const string IoTHubEventHubConsumerGroupKey = "IoTHub:EventHub:ConsumerGroup";
        internal const string DPSConnectionStringKey = "IoTDPS:ConnectionString";
        internal const string DPSServiceEndpointKey = "IoTDPS:ServiceEndpoint";
        internal const string DPSIDScopeKey = "IoTDPS:IDScope";
        internal const string UseSecurityHeadersKey = "UseSecurityHeaders";
        internal const string PostgreSQLConnectionStringKey = "PostgreSQL:ConnectionString";

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

        internal const string IsLoRaFeatureEnabledKey = "LoRaFeature:Enabled";

        internal const string StorageAccountConnectionStringKey = "StorageAccount:ConnectionString";
        internal const string StorageAccountDeviceModelImageMaxAgeKey = "StorageAccount:DeviceModel:Image:MaxAgeInSeconds";

        internal const string LoRaKeyManagementUrlKey = "LoRaKeyManagement:Url";
        internal const string LoRaKeyManagementCodeKey = "LoRaKeyManagement:Code";
        internal const string LoRaKeyManagementApiVersionKey = "LoRaKeyManagement:ApiVersion";

        internal const string MetricExporterRefreshIntervalKey = "Metrics:ExporterRefreshIntervalInSeconds";
        internal const string MetricLoaderRefreshIntervalKey = "Metrics:LoaderRefreshIntervalInMinutes";

        internal const string SyncDatabaseJobRefreshIntervalKey = "Job:SyncDatabaseJobRefreshIntervalInMinutes";

        internal const string IdeasEnabledKey = "Ideas:Enabled";
        internal const string IdeasUrlKey = "Ideas:Url";
        internal const string IdeasAuthenticationHeaderKey = "Ideas:Authentication:Header";
        internal const string IdeasAuthenticationTokenKey = "Ideas:Authentication:Token";
    }
}
