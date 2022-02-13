// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Shared.Models.V10;

    public class DeviceModelCommandMapper : IDeviceModelCommandMapper
    {
        public DeviceModelCommand GetDeviceModelCommand(TableEntity entity)
        {
            return new DeviceModelCommand
            {
                Name = entity.RowKey,
                Frame = entity[nameof(DeviceModelCommand.Frame)].ToString(),
                Port = int.Parse(entity[nameof(DeviceModelCommand.Port)].ToString()),
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelCommand.IsBuiltin)].ToString()),
            };
        }

        public void UpdateTableEntity(TableEntity commandEntity, DeviceModelCommand element)
        {
            commandEntity[nameof(DeviceModelCommand.Frame)] = element.Frame;
            commandEntity[nameof(DeviceModelCommand.Port)] = element.Port;
            commandEntity[nameof(DeviceModelCommand.IsBuiltin)] = element.IsBuiltin;
        }
    }
}
