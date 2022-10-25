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
    public class ConcentratorMetricExporterService : IJob
    {
        private readonly ILogger<ConcentratorMetricExporterService> logger;
        private readonly PortalMetric portalMetric;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");
        private readonly Counter connectedConcentratorCounter = Metrics.CreateCounter(MetricName.ConnectedConcentratorCount, "Connected concentrators count");

        public ConcentratorMetricExporterService(ILogger<ConcentratorMetricExporterService> logger, PortalMetric portalMetric)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
        }

        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting concentrators metrics");

            this.concentratorCounter.IncTo(this.portalMetric.ConcentratorCount);
            this.connectedConcentratorCounter.IncTo(this.portalMetric.ConnectedConcentratorCount);

            this.logger.LogInformation("End exporting concentrators metrics");

            return Task.CompletedTask;
        }
    }
}
