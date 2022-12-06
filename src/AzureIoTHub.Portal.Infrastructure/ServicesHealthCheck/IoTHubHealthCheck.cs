// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class IoTHubHealthCheck : IHealthCheck
    {
        private readonly RegistryManager registryManager;
        private readonly ServiceClient serviceClient;

        public IoTHubHealthCheck(
             RegistryManager registryManager,
             ServiceClient serviceClient)
        {
            this.registryManager = registryManager;
            this.serviceClient = serviceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _ = await this.serviceClient.GetServiceStatisticsAsync(cancellationToken);

                var query = this.registryManager.CreateQuery("SELECT count() FROM devices");

                if (query == null)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Something went wrong when the registry manager executed the query.");
                }
                _ = await query.GetNextAsJsonAsync();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
