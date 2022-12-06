// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public interface ILoRaWanManagementService
    {
        Task<HttpResponseMessage> CheckAzureFunctionReturn(CancellationToken cancellationToken);
        Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommandDto commandDto);
        Task<RouterConfig> GetRouterConfig(string loRaRegion);
    }
}
