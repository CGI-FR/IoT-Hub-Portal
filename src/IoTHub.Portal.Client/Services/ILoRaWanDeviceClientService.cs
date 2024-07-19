// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;

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
