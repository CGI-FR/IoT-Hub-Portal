// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using Shared.Models.v1._0.LoRaWAN;

    public interface ILoRaWANCommandService
    {
        Task<DeviceModelCommandDto[]> GetDeviceModelCommandsFromModel(string id);
        Task PostDeviceModelCommands(string id, DeviceModelCommandDto[] commands);
        Task ExecuteLoRaWANCommand(string deviceId, string commandId);
    }
}
