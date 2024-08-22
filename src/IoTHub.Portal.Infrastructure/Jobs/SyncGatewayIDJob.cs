// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Shared.Models.v1._0.LoRaWAN;

    [DisallowConcurrentExecution]
    public class SyncGatewayIDJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly LoRaGatewayIDList gatewayIdList;

        private readonly ILogger<SyncGatewayIDJob> logger;

        public SyncGatewayIDJob(
            IExternalDeviceService externalDeviceService,
            LoRaGatewayIDList gatewayIdList,
            ILogger<SyncGatewayIDJob> logger)
        {
            this.externalDeviceService = externalDeviceService;
            this.gatewayIdList = gatewayIdList;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync GatewayID job");

                await SyncGatewayID();

                this.logger.LogInformation("End of sync GatewayID job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync GatewayID job has failed");
            }
        }

        private async Task SyncGatewayID()
        {
            this.gatewayIdList.GatewayIds = await this.externalDeviceService.GetAllGatewayID();
        }
    }
}
