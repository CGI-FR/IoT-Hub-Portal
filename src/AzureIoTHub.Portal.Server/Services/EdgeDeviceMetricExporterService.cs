// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Shared.Constants;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Quartz;
    using Shared.Models.v1._0;

    [DisallowConcurrentExecution]
    public class EdgeDeviceMetricExporterService : IJob
    {
        private readonly ILogger<EdgeDeviceMetricExporterService> logger;
        private readonly PortalMetric portalMetric;

        private readonly Counter edgeDeviceCounter = Metrics.CreateCounter(MetricName.EdgeDeviceCount, "Edge devices count");
        private readonly Counter connectedEdgeDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedEdgeDeviceCount, "Connected edge devices count");
        private readonly Counter failedDeploymentCount = Metrics.CreateCounter(MetricName.FailedDeploymentCount, "Failed deployments count");

        public EdgeDeviceMetricExporterService(ILogger<EdgeDeviceMetricExporterService> logger, PortalMetric portalMetric)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
        }

        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting edge devices metrics");

            this.edgeDeviceCounter.IncTo(this.portalMetric.EdgeDeviceCount);
            this.connectedEdgeDeviceCounter.IncTo(this.portalMetric.ConnectedEdgeDeviceCount);

            this.failedDeploymentCount.IncTo(this.portalMetric.FailedDeploymentCount);

            this.logger.LogInformation("End exporting edge devices metrics");

            return Task.CompletedTask;
        }
    }
}
