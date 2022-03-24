// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.ServicesHealthCheck
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
                await ExecuteServiceConnectionCheckAsync(cancellationToken);

                await ExecuteRegistryReadCheckAsync();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task ExecuteServiceConnectionCheckAsync(CancellationToken cancellationToken)
        {
            _ = await this.serviceClient.GetServiceStatisticsAsync(cancellationToken);
        }

        private async Task ExecuteRegistryReadCheckAsync()
        {
            var query = this.registryManager.CreateQuery("SELECT count() FROM devices", 1);
            _ = await query.GetNextAsJsonAsync();
        }
    }
}
