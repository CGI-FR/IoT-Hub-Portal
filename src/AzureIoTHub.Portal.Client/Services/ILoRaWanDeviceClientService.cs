// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10;

    public interface ILoRaWanDeviceClientService
    {
        Task<LoRaDeviceDetails> GetDevice(string deviceId);

        Task CreateDevice(LoRaDeviceDetails device);

        Task UpdateDevice(LoRaDeviceDetails device);

        Task DeleteDevice(string deviceId);

        Task ExecuteCommand(string deviceId, string commandId);

        Task<LoRaGatewayIDList> GetGatewayIdList();

        Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId);
    }
}
