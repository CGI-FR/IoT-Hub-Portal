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
        protected const string PortalNameKey = "SiteName";
        protected const string IoTHubConnectionStringKey = "IoTHub:ConnectionString";
        protected const string DPSConnectionStringKey = "IoTDPS:ConnectionString";
        protected const string DPSServiceEndpointKey = "IoTDPS:ServiceEndpoint";

        protected const string OIDCScopeKey = "OIDC:Scope";
        protected const string OIDCAuthorityKey = "OIDC:Authority";
        protected const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
        protected const string OIDCClientIdKey = "OIDC:ClientId";
        protected const string OIDCApiClientIdKey = "OIDC:ApiClientId";

        protected const string IsLoRaFeatureEnabledKey = "LoRaFeature:Enabled";

        protected const string StorageAccountConnectionStringKey = "StorageAccount:ConnectionString";
        protected const string StorageAccountBlobContainerNameKey = "StorageAccount:BlobContainerName";
        protected const string StorageAccountBlobContainerPartitionKeyKey = "StorageAccount:BlobContainerPartitionKey";

        protected const string LoRaKeyManagementUrlKey = "LoRaKeyManagement:Url";
        protected const string LoRaKeyManagementCodeKey = "LoRaKeyManagement:Code";
        protected const string LoRaRegionRouterConfigUrlKey = "LoRaRegionRouterConfig:Url";

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
