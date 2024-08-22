// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Models.v1._0.LoRaWAN;

    public interface ILoRaWanManagementService
    {
        Task<HttpResponseMessage> CheckAzureFunctionReturn(CancellationToken cancellationToken);
        Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommandDto commandDto);
        Task<RouterConfig?> GetRouterConfig(string loRaRegion);
    }
}
