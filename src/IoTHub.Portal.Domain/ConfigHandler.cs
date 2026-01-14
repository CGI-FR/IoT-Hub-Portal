// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain
{
    public abstract class ConfigHandler
    {
        public abstract string AzureIoTHubConnectionString { get; }

        public abstract string AzureIoTHubEventHubEndpoint { get; }

        public abstract string AzureIoTHubEventHubConsumerGroup { get; }

        public abstract string AzureDpsConnectionString { get; }

        public abstract string AzureDpsEndpoint { get; }

        public abstract string AzureDpsScopeId { get; }

        public abstract string AzureStorageAccountConnectionString { get; }

        public abstract int StorageAccountDeviceModelImageMaxAge { get; }

        public abstract bool UseSecurityHeaders { get; }

        public abstract string OidcScope { get; }

        public abstract string OidcApiClientId { get; }

        public abstract string OidcClientId { get; }

        public abstract string OidcMetadataUrl { get; }

        public abstract string OidcAuthority { get; }

        public abstract bool OidcValidateIssuer { get; }

        public abstract bool OidcValidateAudience { get; }

        public abstract bool OidcValidateLifetime { get; }

        public abstract bool OidcValidateIssuerSigningKey { get; }

        public abstract bool OidcValidateActor { get; }

        public abstract bool OidcValidateTokenReplay { get; }

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

        public abstract string PostgreSqlConnectionString { get; }

        public abstract string MySqlConnectionString { get; }

        public abstract string DbProvider { get; }
        public abstract string CloudProvider { get; }
        public abstract string AwsAccess { get; }
        public abstract string AwsAccessSecret { get; }
        public abstract string AwsRegion { get; }
        public abstract string Awss3StorageConnectionString { get; }
        public abstract string AwsBucketName { get; }
        public abstract string AwsAccountId { get; }
        public abstract string AwsGreengrassCoreTokenExchangeRoleAliasName { get; }
        public abstract IEnumerable<string> AwsGreengrassRequiredRoles { get; }

        public abstract int SendCommandsToDevicesIntervalInMinutes { get; }
    }
}
