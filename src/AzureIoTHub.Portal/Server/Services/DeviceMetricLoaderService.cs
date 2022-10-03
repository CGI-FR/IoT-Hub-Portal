// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v1._0;

    [DisallowConcurrentExecution]
    public class DeviceMetricLoaderService : IJob
    {
        private readonly ILogger<DeviceMetricLoaderService> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;

        public DeviceMetricLoaderService(ILogger<DeviceMetricLoaderService> logger, PortalMetric portalMetric, IExternalDeviceService externalDeviceService)
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
