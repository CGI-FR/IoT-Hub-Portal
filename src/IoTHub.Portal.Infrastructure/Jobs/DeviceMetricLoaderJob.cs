// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Application.Services;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v10;

    [DisallowConcurrentExecution]
    public class DeviceMetricLoaderJob : IJob
    {
        private readonly ILogger<DeviceMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;

        public DeviceMetricLoaderJob(ILogger<DeviceMetricLoaderJob> logger, PortalMetric portalMetric, IExternalDeviceService externalDeviceService)
        {
            this.logger = logger;
            this.portalMetric = portalMetric;
            this.externalDeviceService = externalDeviceService;
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
                this.portalMetric.DeviceCount = await this.externalDeviceService.GetDevicesCount();
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
                this.portalMetric.ConnectedDeviceCount = await this.externalDeviceService.GetConnectedDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected devices count metric: {e.Detail}", e);
            }
        }
    }
}
