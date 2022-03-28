// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.ServicesHealthCheck
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class StorageAccountHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly ConfigHandler configHandler;

        public StorageAccountHealthCheck(BlobServiceClient blobServiceClient,
            ConfigHandler configHandler)
        {
            this.blobServiceClient = blobServiceClient;
            this.configHandler = configHandler;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                if (this.configHandler.StorageAccountBlobContainerName != null)
                {
                    var containerClient = this.blobServiceClient.GetBlobContainerClient(this.configHandler.StorageAccountBlobContainerName);

                    if (!await containerClient.ExistsAsync(cancellationToken))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Container '{this.configHandler.StorageAccountBlobContainerName}' not exists");
                    }

                    _ = await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
