// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using Microsoft.Extensions.Configuration;

    internal class DevelopmentConfigHandler : ConfigHandler
    {
        private readonly IConfiguration config;

        internal DevelopmentConfigHandler(IConfiguration config)
        {
            this.config = config;
        }

        internal override string PortalName => this.config[PortalNameKey];

        internal override int MetricExporterRefreshIntervalInSeconds => this.config.GetValue(MetricExporterRefreshIntervalKey, 30);

        internal override int MetricLoaderRefreshIntervalInMinutes => this.config.GetValue(MetricLoaderRefreshIntervalKey, 10);

        internal override string IoTHubConnectionString => this.config[IoTHubConnectionStringKey];

        internal override string DPSConnectionString => this.config[DPSConnectionStringKey];

        internal override string DPSEndpoint => this.config[DPSServiceEndpointKey];

        internal override string DPSScopeID => this.config[DPSIDScopeKey];

        internal override string StorageAccountConnectionString => this.config[StorageAccountConnectionStringKey];

        internal override bool UseSecurityHeaders => this.config.GetValue(UseSecurityHeadersKey, true);

        internal override string OIDCScope => this.config[OIDCScopeKey];

        internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

        internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

        internal override string OIDCClientId => this.config[OIDCClientIdKey];

        internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

        internal override bool OIDCValidateIssuer => this.config.GetValue(OIDCValidateIssuerKey, true);

        internal override bool OIDCValidateAudience => this.config.GetValue(OIDCValidateAudienceKey, true);

        internal override bool OIDCValidateLifetime => this.config.GetValue(OIDCValidateLifetimeKey, true);

        internal override bool OIDCValidateIssuerSigningKey => this.config.GetValue(OIDCValidateIssuerSigningKeyKey, true);

        internal override bool OIDCValidateActor => this.config.GetValue(OIDCValidateActorKey, false);

        internal override bool OIDCValidateTokenReplay => this.config.GetValue(OIDCValidateTokenReplayKey, false);

        internal override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "true");

        internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

        internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];

        internal override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

        internal override string LoRaKeyManagementCode => this.config[LoRaKeyManagementCodeKey];
    }
}
