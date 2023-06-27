// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using IoTHub.Portal.Domain.Shared.Constants;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Quartz;
    using Shared.Models.v1._0;
    using System.Threading.Tasks;

    [DisallowConcurrentExecution]
    public class EdgeDeviceMetricExporterJob : IJob
    {
        private readonly ILogger<EdgeDeviceMetricExporterJob> logger;
        private readonly PortalMetricDto portalMetric;

        private readonly Counter edgeDeviceCounter = Metrics.CreateCounter(MetricName.EdgeDeviceCount, "Edge devices count");
        private readonly Counter connectedEdgeDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedEdgeDeviceCount, "Connected edge devices count");
        private readonly Counter failedDeploymentCount = Metrics.CreateCounter(MetricName.FailedDeploymentCount, "Failed deployments count");

        public EdgeDeviceMetricExporterJob(ILogger<EdgeDeviceMetricExporterJob> logger, PortalMetricDto portalMetric)
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
