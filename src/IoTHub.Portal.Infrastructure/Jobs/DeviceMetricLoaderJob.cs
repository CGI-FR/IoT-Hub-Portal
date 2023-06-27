// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v1._0;

    [DisallowConcurrentExecution]
    public class DeviceMetricLoaderJob : IJob
    {
        private readonly ILogger<DeviceMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IDeviceRepository deviceRepository;

        public DeviceMetricLoaderJob(ILogger<DeviceMetricLoaderJob> logger, PortalMetric portalMetric, IDeviceRepository deviceRepository)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
            this.deviceRepository = deviceRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading devices metrics");

            await LoadDevicesCountMetric();
            await LoadConnectedDevicesCountMetric();

            this.logger.LogInformation("End loading devices metrics");
        }

        private async Task LoadDevicesCountMetric()
        {
            try
            {
                this.portalMetric.DeviceCount = await this.deviceRepository.CountAsync();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadConnectedDevicesCountMetric()
        {
            try
            {
                this.portalMetric.ConnectedDeviceCount = await this.deviceRepository.CountAsync(c => c.IsConnected);
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected devices count metric: {e.Detail}", e);
            }
        }
    }
}
