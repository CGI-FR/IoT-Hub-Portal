// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncGatewayIdJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly LoRaGatewayIdList gatewayIdList;

        private readonly ILogger<SyncGatewayIdJob> logger;

        public SyncGatewayIdJob(
            IExternalDeviceService externalDeviceService,
            LoRaGatewayIdList gatewayIdList,
            ILogger<SyncGatewayIdJob> logger)
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

                await SyncGatewayId();

                this.logger.LogInformation("End of sync GatewayID job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync GatewayID job has failed");
            }
        }

        private async Task SyncGatewayId()
        {
            this.gatewayIdList.GatewayIds = await this.externalDeviceService.GetAllGatewayId();
        }
    }
}
