// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using Microsoft.Extensions.Configuration;

    internal class ProductionConfigHandler : ConfigHandler
    {
        private readonly IConfiguration config;

        internal ProductionConfigHandler(IConfiguration config)
        {
            this.config = config;
        }

        internal override string PortalName => this.config[PortalNameKey];

        internal override string IoTHubConnectionString => this.config.GetConnectionString(IoTHubConnectionStringKey);

        internal override string DPSConnectionString => this.config.GetConnectionString(DPSConnectionStringKey);

        internal override string DPSEndpoint => this.config[DPSServiceEndpointKey];

        internal override string DPSScopeID => this.config[DPSIDScopeKey];

        internal override string StorageAccountConnectionString => this.config.GetConnectionString(StorageAccountConnectionStringKey);

        internal override string OIDCScope => this.config[OIDCScopeKey];

        internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

        internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

        internal override string OIDCClientId => this.config[OIDCClientIdKey];

        internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

        internal override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "true");

        internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

        internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];

        internal override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

        internal override string LoRaKeyManagementCode => this.config.GetConnectionString(LoRaKeyManagementCodeKey);

        internal override string LoRaRegionRouterConfigUrl => this.config[LoRaRegionRouterConfigUrlKey];
    }
}
