// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v1._0;

    [DisallowConcurrentExecution]
    public class EdgeDeviceMetricLoaderService : IJob
    {
        private readonly ILogger<EdgeDeviceMetricLoaderService> logger;
        private readonly PortalMetric portalMetric;
        private readonly IDeviceService deviceService;
        private readonly IConfigService configService;

        public EdgeDeviceMetricLoaderService(ILogger<EdgeDeviceMetricLoaderService> logger, PortalMetric portalMetric, IDeviceService deviceService, IConfigService configService)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
            this.deviceService = deviceService;
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
                this.portalMetric.EdgeDeviceCount = await deviceService.GetEdgeDevicesCount();
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
                this.portalMetric.ConnectedEdgeDeviceCount = await deviceService.GetConnectedEdgeDevicesCount();
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
