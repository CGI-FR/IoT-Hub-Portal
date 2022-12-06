// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Infrastructure.Factories;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class TableStorageHealthCheck : IHealthCheck
    {
        private readonly ITableClientFactory tableClientFactory;

        public TableStorageHealthCheck(ITableClientFactory tableClientFactory)
        {
            this.tableClientFactory = tableClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            const string partitionKey = "0";
            const string rowKey = "1";

            try
            {
                var tableClient = this.tableClientFactory.GetTemplatesHealthCheck();

                _ = await tableClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                var entity = new TableEntity(partitionKey, rowKey)
                {
                    {"key","value" }
                };

                _ = await tableClient.AddEntityAsync(entity, cancellationToken: cancellationToken);
                _ = await tableClient.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);
                _ = await tableClient.DeleteAsync(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

    }
}
