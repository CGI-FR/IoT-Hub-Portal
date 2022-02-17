// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;

    public interface IDeviceModelCommandsManager
    {
        List<Command> RetrieveCommands(string deviceModel);

        List<DeviceModelCommand> RetrieveDeviceModelCommands(string deviceModel);
    }
}
