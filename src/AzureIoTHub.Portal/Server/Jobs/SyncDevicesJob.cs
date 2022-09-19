// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Jobs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.Azure.Devices.Shared;
    using Quartz;

    public class SyncDevicesJob : IJob
    {
        private readonly IDeviceService deviceService;

        public SyncDevicesJob(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var twins = new List<Twin>();
            var continuationToken = string.Empty;

            int totalTwinDevices;
            do
            {
                var result = await this.deviceService.GetAllDevice(continuationToken: continuationToken, pageSize: 100);
                twins.AddRange(result.Items);

                totalTwinDevices = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinDevices > twins.Count);

            throw new System.NotImplementedException();
        }
    }
}
