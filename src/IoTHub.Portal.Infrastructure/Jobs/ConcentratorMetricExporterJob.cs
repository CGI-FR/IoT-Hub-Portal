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
    public class ConcentratorMetricExporterJob : IJob
    {
        private readonly ILogger<ConcentratorMetricExporterJob> logger;
        private readonly PortalMetricDto portalMetric;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");

        public ConcentratorMetricExporterJob(ILogger<ConcentratorMetricExporterJob> logger, PortalMetricDto portalMetric)
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
