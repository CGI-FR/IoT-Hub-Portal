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
    public class DeviceMetricExporterJob : IJob
    {
        private readonly ILogger<DeviceMetricExporterJob> logger;
        private readonly PortalMetric portalMetric;

        private readonly Counter deviceCounter = Metrics.CreateCounter(MetricName.DeviceCount, "Devices count");
        private readonly Counter connectedDeviceCounter = Metrics.CreateCounter(MetricName.ConnectedDeviceCount, "Connected devices count");

        public DeviceMetricExporterJob(ILogger<DeviceMetricExporterJob> logger, PortalMetric portalMetric)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
        }

        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting devices metrics");

            this.deviceCounter.IncTo(this.portalMetric.DeviceCount);
            this.connectedDeviceCounter.IncTo(this.portalMetric.ConnectedDeviceCount);

            this.logger.LogInformation("End exporting devices metrics");

            return Task.CompletedTask;
        }
    }
}
