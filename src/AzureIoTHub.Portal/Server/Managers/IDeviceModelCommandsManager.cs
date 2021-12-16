// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.Device;

    public interface IDeviceModelCommandsManager
    {
        List<Command> RetrieveCommands(string deviceModel);

        List<DeviceModelCommand> RetrieveDeviceModelCommands(string deviceModel);
    }
}
