// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Shared.Constants;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Quartz;
    using Shared.Models.v10;

    [DisallowConcurrentExecution]
    public class ConcentratorMetricExporterJob : IJob
    {
        private readonly ILogger<ConcentratorMetricExporterJob> logger;
        private readonly PortalMetric portalMetric;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");

        public ConcentratorMetricExporterJob(ILogger<ConcentratorMetricExporterJob> logger, PortalMetric portalMetric)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
        }

        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting concentrators metrics");

            this.concentratorCounter.IncTo(this.portalMetric.ConcentratorCount);

            this.logger.LogInformation("End exporting concentrators metrics");

            return Task.CompletedTask;
        }
    }
}
