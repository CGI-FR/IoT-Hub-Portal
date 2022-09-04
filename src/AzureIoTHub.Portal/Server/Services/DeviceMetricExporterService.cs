// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain;
    using Constants;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Shared.Models.v1._0;

    public class DeviceMetricExporterService : BackgroundService
    {
        private readonly ILogger<DeviceMetricExporterService> logger;
        private readonly ConfigHandler configHandler;
        private readonly PortalMetric portalMetric;

        private readonly Counter deviceCounter = Metrics.CreateCounter(MetricName.DeviceCount, "Devices count");
        private readonly Counter connectedDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedDeviceCount, "Connected devices count");

        public DeviceMetricExporterService(ILogger<DeviceMetricExporterService> logger, ConfigHandler configHandler, PortalMetric portalMetric)
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
                this.logger.LogInformation("Start exporting devices metrics");

                this.deviceCounter.IncTo(this.portalMetric.DeviceCount);
                this.connectedDeviceCounter.IncTo(this.portalMetric.ConnectedDeviceCount);

                this.logger.LogInformation("End exporting devices metrics");
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
