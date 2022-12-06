// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class LoRaManagementKeyFacadeHealthCheck : IHealthCheck
    {
        private readonly ILoRaWanManagementService loRaWanManagementService;

        public LoRaManagementKeyFacadeHealthCheck(ILoRaWanManagementService loRaWanManagementService)
        {
            this.loRaWanManagementService = loRaWanManagementService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result  = await this.loRaWanManagementService.CheckAzureFunctionReturn(cancellationToken);

                _ = result.EnsureSuccessStatusCode();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
