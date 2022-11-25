// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain
{
    public abstract class ConfigHandler
    {
        public abstract string IoTHubConnectionString { get; }

        public abstract string IoTHubEventHubEndpoint { get; }

        public abstract string IoTHubEventHubConsumerGroup { get; }

        public abstract string DPSConnectionString { get; }

        public abstract string DPSEndpoint { get; }

        public abstract string DPSScopeID { get; }

        public abstract string StorageAccountConnectionString { get; }

        public abstract int StorageAccountDeviceModelImageMaxAge { get; }

        public abstract bool UseSecurityHeaders { get; }

        public abstract string OIDCScope { get; }

        public abstract string OIDCApiClientId { get; }

        public abstract string OIDCClientId { get; }

        public abstract string OIDCMetadataUrl { get; }

        public abstract string OIDCAuthority { get; }

        public abstract bool OIDCValidateIssuer { get; }

        public abstract bool OIDCValidateAudience { get; }

        public abstract bool OIDCValidateLifetime { get; }

        public abstract bool OIDCValidateIssuerSigningKey { get; }

        public abstract bool OIDCValidateActor { get; }

        public abstract bool OIDCValidateTokenReplay { get; }

        public abstract bool IsLoRaEnabled { get; }

        public abstract string LoRaKeyManagementUrl { get; }

        public abstract string LoRaKeyManagementCode { get; }

        public abstract string LoRaKeyManagementApiVersion { get; }

        public abstract string PortalName { get; }

        public abstract int SyncDatabaseJobRefreshIntervalInMinutes { get; }

        public abstract int MetricExporterRefreshIntervalInSeconds { get; }

        public abstract int MetricLoaderRefreshIntervalInMinutes { get; }

        public abstract bool IdeasEnabled { get; }

        public abstract string IdeasUrl { get; }

        public abstract string IdeasAuthenticationHeader { get; }

        public abstract string IdeasAuthenticationToken { get; }

        public abstract string PostgreSQLConnectionString { get; }
    }
}
