// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using AzureIoTHub.Portal.Domain.Repositories;
    using Domain.Entities;

    public class DeviceModelCommandRepository : GenericRepository<DeviceModelCommand>, IDeviceModelCommandRepository
    {
        public DeviceModelCommandRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<bool> DeviceModelCommandExists(string deviceModelCommandId)
        {
            return await this.context.DeviceModelCommands.FindAsync(deviceModelCommandId) != null;
        }

        public async Task<DeviceModelCommand> CreateDeviceModelCommand(DeviceModelCommand deviceModelCommand)
        {
            var deviceModelCommandEntity = await this.context.DeviceModelCommands.AddAsync(deviceModelCommand);
            return deviceModelCommandEntity.Entity;
        }

        public async Task<DeviceModelCommand> UpdateDeviceModelCommand(DeviceModelCommand deviceModelCommand)
        {
            var deviceModelCommandEntity = this.context.Update(deviceModelCommand);
            return deviceModelCommandEntity.Entity;
        }

        public async Task DeleteDeviceModelCommand(string deviceModelCommandId)
        {
            if (!await DeviceModelCommandExists(deviceModelCommandId))
            {
                return;
            }

            var deviceModelCommandEntity = new DeviceModelCommand
            {
                Id = deviceModelCommandId
            };

            _ = this.context.Attach(deviceModelCommandEntity);
            _ = this.context.Remove(deviceModelCommandEntity);
        }
    }
}
