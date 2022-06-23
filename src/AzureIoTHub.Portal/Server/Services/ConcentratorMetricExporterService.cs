// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Constants;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Shared.Models.v1._0;

    public class ConcentratorMetricExporterService : BackgroundService
    {
        private readonly ILogger<ConcentratorMetricExporterService> logger;
        private readonly ConfigHandler configHandler;
        private readonly PortalMetric portalMetric;

        private readonly Counter concentratorCounter = Metrics.CreateCounter(MetricName.ConcentratorCount, "Concentrators count");
        private readonly Counter connectedConcentratorCounter = Metrics.CreateCounter(MetricName.ConnectedConcentratorCount, "Connected concentrators count");

        public ConcentratorMetricExporterService(ILogger<ConcentratorMetricExporterService> logger, ConfigHandler configHandler, PortalMetric portalMetric)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.portalMetric = portalMetric;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(this.configHandler.MetricExporterRefreshIntervalInSeconds));

            do
            {
                this.logger.LogInformation("Start exporting concentrators metrics");

                this.concentratorCounter.IncTo(this.portalMetric.ConcentratorCount);
                this.connectedConcentratorCounter.IncTo(this.portalMetric.ConnectedConcentratorCount);

                this.logger.LogInformation("End exporting concentrators metrics");
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
