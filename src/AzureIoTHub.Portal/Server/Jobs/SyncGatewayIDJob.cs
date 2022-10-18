// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncGatewayIDJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly GatewayIdList gatewayIdList;

        private readonly ILogger<SyncConcentratorsJob> logger;

        public SyncGatewayIDJob(
            IExternalDeviceService externalDeviceService,
            GatewayIdList gatewayIdList,
            ILogger<SyncConcentratorsJob> logger)
        {
            this.externalDeviceService = externalDeviceService;
            this.gatewayIdList = gatewayIdList;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync job");

                await SyncGatewayID();

                this.logger.LogInformation("End of sync job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync job has failed");
            }
        }

        private async Task SyncGatewayID()
        {
            var test = await this.externalDeviceService.GetAllGatewayID();
            this.gatewayIdList.GatewayIds = test;
        }
    }
}
