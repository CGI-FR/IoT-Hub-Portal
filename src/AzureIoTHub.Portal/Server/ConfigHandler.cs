// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public abstract class ConfigHandler
    {
        internal const string PortalNameKey = "PortalName";
        internal const string IoTHubConnectionStringKey = "IoTHub:ConnectionString";
        internal const string DPSConnectionStringKey = "IoTDPS:ConnectionString";
        internal const string DPSServiceEndpointKey = "IoTDPS:ServiceEndpoint";
        internal const string DPSIDScopeKey = "IoTDPS:IDScope";
        internal const string UseSecurityHeadersKey = "UseSecurityHeaders";

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

        internal const string IdeasEnabledKey = "Ideas:Enabled";
        internal const string IdeasUrlKey = "Ideas:Url";
        internal const string IdeasAuthenticationHeaderKey = "Ideas:Authentication:Header";
        internal const string IdeasAuthenticationTokenKey = "Ideas:Authentication:Token";

        internal static ConfigHandler Create(IWebHostEnvironment env, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(env, nameof(env));
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            if (env.IsProduction())
            {
                return new ProductionConfigHandler(config);
            }

            return new DevelopmentConfigHandler(config);
        }

        internal abstract string IoTHubConnectionString { get; }

        internal abstract string DPSConnectionString { get; }

        internal abstract string DPSEndpoint { get; }

        internal abstract string DPSScopeID { get; }

        internal abstract string StorageAccountConnectionString { get; }

        internal abstract int StorageAccountDeviceModelImageMaxAge { get; }

        internal abstract bool UseSecurityHeaders { get; }

        internal abstract string OIDCScope { get; }

        internal abstract string OIDCApiClientId { get; }

        internal abstract string OIDCClientId { get; }

        internal abstract string OIDCMetadataUrl { get; }

        internal abstract string OIDCAuthority { get; }

        internal abstract bool OIDCValidateIssuer { get; }

        internal abstract bool OIDCValidateAudience { get; }

        internal abstract bool OIDCValidateLifetime { get; }

        internal abstract bool OIDCValidateIssuerSigningKey { get; }

        internal abstract bool OIDCValidateActor { get; }

        internal abstract bool OIDCValidateTokenReplay { get; }

        internal abstract bool IsLoRaEnabled { get; }

        internal abstract string LoRaKeyManagementUrl { get; }

        internal abstract string LoRaKeyManagementCode { get; }

        internal abstract string LoRaKeyManagementApiVersion { get; }

        internal abstract string PortalName { get; }

        internal abstract int MetricExporterRefreshIntervalInSeconds { get; }

        internal abstract int MetricLoaderRefreshIntervalInMinutes { get; }

        internal abstract bool IdeasEnabled { get; }

        internal abstract string IdeasUrl { get; }

        internal abstract string IdeasAuthenticationHeader { get; }

        internal abstract string IdeasAuthenticationToken { get; }
    }
}
