// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using Microsoft.Extensions.Configuration;

    internal class DevelopmentConfigHandler : ConfigHandlerBase
    {
        private readonly IConfiguration config;

        internal DevelopmentConfigHandler(IConfiguration config)
        {
            this.config = config;
        }

        public override string PortalName => this.config[PortalNameKey];

        public override int SyncDatabaseJobRefreshIntervalInMinutes => this.config.GetValue(SyncDatabaseJobRefreshIntervalKey, 5);

        public override int MetricExporterRefreshIntervalInSeconds => this.config.GetValue(MetricExporterRefreshIntervalKey, 30);

        public override int MetricLoaderRefreshIntervalInMinutes => this.config.GetValue(MetricLoaderRefreshIntervalKey, 10);

        public override string IoTHubConnectionString => this.config[IoTHubConnectionStringKey];

        public override string IoTHubEventHubEndpoint => this.config.GetValue(IoTHubEventHubEndpointKey, string.Empty);

        public override string IoTHubEventHubConsumerGroup => this.config.GetValue(IoTHubEventHubConsumerGroupKey, "iothub-portal");

        public override string DPSConnectionString => this.config[DPSConnectionStringKey];

        public override string DPSEndpoint => this.config[DPSServiceEndpointKey];

        public override string DPSScopeID => this.config[DPSIDScopeKey];

        public override string StorageAccountConnectionString => this.config[StorageAccountConnectionStringKey];

        public override int StorageAccountDeviceModelImageMaxAge => this.config.GetValue(StorageAccountDeviceModelImageMaxAgeKey, 86400);

        public override bool UseSecurityHeaders => this.config.GetValue(UseSecurityHeadersKey, true);

        public override string OIDCScope => this.config[OIDCScopeKey];

        public override string OIDCAuthority => this.config[OIDCAuthorityKey];

        public override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

        public override string OIDCClientId => this.config[OIDCClientIdKey];

        public override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

        public override bool OIDCValidateIssuer => this.config.GetValue(OIDCValidateIssuerKey, true);

        public override bool OIDCValidateAudience => this.config.GetValue(OIDCValidateAudienceKey, true);

        public override bool OIDCValidateLifetime => this.config.GetValue(OIDCValidateLifetimeKey, true);

        public override bool OIDCValidateIssuerSigningKey => this.config.GetValue(OIDCValidateIssuerSigningKeyKey, true);

        public override bool OIDCValidateActor => this.config.GetValue(OIDCValidateActorKey, false);

        public override bool OIDCValidateTokenReplay => this.config.GetValue(OIDCValidateTokenReplayKey, false);

        public override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "true");

        public override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

        public override string LoRaKeyManagementCode => this.config[LoRaKeyManagementCodeKey];

        public override string LoRaKeyManagementApiVersion => this.config[LoRaKeyManagementApiVersionKey];

        public override bool IdeasEnabled => this.config.GetValue(IdeasEnabledKey, false);
        public override string IdeasUrl => this.config.GetValue(IdeasUrlKey, string.Empty);
        public override string IdeasAuthenticationHeader => this.config.GetValue(IdeasAuthenticationHeaderKey, "Ocp-Apim-Subscription-Key");
        public override string IdeasAuthenticationToken => this.config.GetValue(IdeasAuthenticationTokenKey, string.Empty);

        public override string PostgreSQLConnectionString => this.config[PostgreSQLConnectionStringKey];
    }
}
