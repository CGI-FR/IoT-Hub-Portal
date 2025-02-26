// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain
{
    public abstract class ConfigHandler
    {
        public abstract string AzureIoTHubConnectionString { get; }

        public abstract string AzureIoTHubEventHubEndpoint { get; }

        public abstract string AzureIoTHubEventHubConsumerGroup { get; }

        public abstract string AzureDPSConnectionString { get; }

        public abstract string AzureDPSEndpoint { get; }

        public abstract string AzureDPSScopeID { get; }

        public abstract string AzureStorageAccountConnectionString { get; }

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

        public abstract string AzureLoRaKeyManagementUrl { get; }

        public abstract string AzureLoRaKeyManagementCode { get; }

        public abstract string AzureLoRaKeyManagementApiVersion { get; }

        public abstract string PortalName { get; }

        public abstract int SyncDatabaseJobRefreshIntervalInMinutes { get; }

        public abstract int MetricExporterRefreshIntervalInSeconds { get; }

        public abstract int MetricLoaderRefreshIntervalInMinutes { get; }

        public abstract bool IdeasEnabled { get; }

        public abstract string IdeasUrl { get; }

        public abstract string IdeasAuthenticationHeader { get; }

        public abstract string IdeasAuthenticationToken { get; }

        public abstract string PostgreSQLConnectionString { get; }

        public abstract string MySQLConnectionString { get; }

        public abstract string DbProvider { get; }
        public abstract string CloudProvider { get; }
        public abstract string AWSAccess { get; }
        public abstract string AWSAccessSecret { get; }
        public abstract string AWSRegion { get; }
        public abstract string AWSS3StorageConnectionString { get; }
        public abstract string AWSBucketName { get; }
        public abstract string AWSAccountId { get; }
        public abstract string AWSGreengrassCoreTokenExchangeRoleAliasName { get; }
        public abstract IEnumerable<string> AWSGreengrassRequiredRoles { get; }

        public abstract int SendCommandsToDevicesIntervalInMinutes { get; }
    }
}
