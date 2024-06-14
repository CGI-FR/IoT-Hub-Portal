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
    public class EdgeDeviceMetricLoaderJob : IJob
    {
        private readonly ILogger<EdgeDeviceMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IConfigService configService;

        public EdgeDeviceMetricLoaderJob(ILogger<EdgeDeviceMetricLoaderJob> logger, PortalMetric portalMetric, IExternalDeviceService externalDeviceService, IConfigService configService)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
            this.externalDeviceService = externalDeviceService;
            this.configService = configService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading edge devices metrics");

            await LoadEdgeDevicesCountMetric();
            await LoadConnectedEdgeDevicesCountMetric();
            await LoadFailedDeploymentsCountMetric();

            this.logger.LogInformation("End loading edge devices metrics");

        }

        private async Task LoadEdgeDevicesCountMetric()
        {
            try
            {
                this.portalMetric.EdgeDeviceCount = await this.externalDeviceService.GetEdgeDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load edge devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadConnectedEdgeDevicesCountMetric()
        {
            try
            {
                this.portalMetric.ConnectedEdgeDeviceCount = await this.externalDeviceService.GetConnectedEdgeDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected edge devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadFailedDeploymentsCountMetric()
        {
            try
            {
                this.portalMetric.FailedDeploymentCount = await configService.GetFailedDeploymentsCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load failed deployments count metric: {e.Detail}", e);
            }
        }
    }
}
