// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.ServicesHealthCheck
{
    using global::Azure.Storage.Blobs;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class StorageAccountHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient blobServiceClient;

        private const string BlobContainerName = "Health";

        public StorageAccountHealthCheck(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = this.blobServiceClient.GetBlobContainerClient(BlobContainerName);

                // Try to create the container
                _ = await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                // Try to delete the container
                _ = await containerClient.DeleteAsync(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
