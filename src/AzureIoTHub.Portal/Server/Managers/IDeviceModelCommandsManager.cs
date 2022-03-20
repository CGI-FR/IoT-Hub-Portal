// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.ObjectModel;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public interface IDeviceModelCommandsManager
    {
        ReadOnlyCollection<Command> RetrieveCommands(string deviceModel);

        ReadOnlyCollection<DeviceModelCommand> RetrieveDeviceModelCommands(string deviceModel);
    }
}
