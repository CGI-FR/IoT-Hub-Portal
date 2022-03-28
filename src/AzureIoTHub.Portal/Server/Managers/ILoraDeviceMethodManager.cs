// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public interface ILoraDeviceMethodManager
    {
        Task<HttpResponseMessage> ExecuteLoRaDeviceMessage(string deviceId, DeviceModelCommand command);
        Task<HttpResponseMessage> CheckAzureFunctionReturn(CancellationToken cancellationToken);
    }
}
