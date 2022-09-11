// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Repositories
{
    using Entities;

    public interface IDeviceModelCommandRepository : IRepository<DeviceModelCommand>
    {
        Task<bool> DeviceModelCommandExists(string deviceModelCommandId);

        Task<DeviceModelCommand> CreateDeviceModelCommand(DeviceModelCommand deviceModelCommand);

        Task<DeviceModelCommand> UpdateDeviceModelCommand(DeviceModelCommand deviceModelCommand);

        Task DeleteDeviceModelCommand(string deviceModelCommandId);
    }
}
