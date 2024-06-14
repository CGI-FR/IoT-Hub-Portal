// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Application.Services;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v10;

    [DisallowConcurrentExecution]
    public class ConcentratorMetricLoaderJob : IJob
    {
        private readonly ILogger<ConcentratorMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;

        public ConcentratorMetricLoaderJob(ILogger<ConcentratorMetricLoaderJob> logger, PortalMetric portalMetric, IExternalDeviceService externalDeviceService)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
            this.externalDeviceService = externalDeviceService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading concentrators metrics");

            await LoadConcentratorsCountMetric();

            this.logger.LogInformation("End loading concentrators metrics");
        }

        private async Task LoadConcentratorsCountMetric()
        {
            try
            {
                this.portalMetric.ConcentratorCount = await this.externalDeviceService.GetConcentratorsCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load concentrators count metric: {e.Detail}", e);
            }
        }
    }
}
