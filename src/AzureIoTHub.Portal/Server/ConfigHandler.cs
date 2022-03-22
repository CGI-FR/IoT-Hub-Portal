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

        internal const string OIDCScopeKey = "OIDC:Scope";
        internal const string OIDCAuthorityKey = "OIDC:Authority";
        internal const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
        internal const string OIDCClientIdKey = "OIDC:ClientId";
        internal const string OIDCApiClientIdKey = "OIDC:ApiClientId";

        internal const string IsLoRaFeatureEnabledKey = "LoRaFeature:Enabled";

        internal const string StorageAccountConnectionStringKey = "StorageAccount:ConnectionString";
        internal const string StorageAccountBlobContainerNameKey = "StorageAccount:BlobContainerName";
        internal const string StorageAccountBlobContainerPartitionKeyKey = "StorageAccount:BlobContainerPartitionKey";

        internal const string LoRaKeyManagementUrlKey = "LoRaKeyManagement:Url";
        internal const string LoRaKeyManagementCodeKey = "LoRaKeyManagement:Code";
        internal const string LoRaRegionRouterConfigUrlKey = "LoRaRegionRouterConfig:Url";

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

        internal abstract string OIDCScope { get; }

        internal abstract string OIDCApiClientId { get; }

        internal abstract string OIDCClientId { get; }

        internal abstract string OIDCMetadataUrl { get; }

        internal abstract string OIDCAuthority { get; }

        internal abstract bool IsLoRaEnabled { get; }

        internal abstract string StorageAccountBlobContainerName { get; }

        internal abstract string StorageAccountBlobContainerPartitionKey { get; }

        internal abstract string LoRaKeyManagementUrl { get; }

        internal abstract string LoRaKeyManagementCode { get; }

        internal abstract string LoRaRegionRouterConfigUrl { get; }

        internal abstract string PortalName { get; }
    }
}
