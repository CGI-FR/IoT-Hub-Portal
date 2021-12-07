// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Shared.Models;

    public class SensorCommandMapper : ISensorCommandMapper
    {
        public SensorCommand GetSensorCommand(TableEntity entity)
        {
            return new SensorCommand
            {
                CommandId = entity.RowKey,
                Frame = entity[nameof(SensorCommand.Frame)].ToString(),
                Port = int.Parse(entity[nameof(SensorCommand.Port)].ToString())
            };
        }

        public void UpdateTableEntity(TableEntity commandEntity, SensorCommand element)
        {
            // commandEntity[nameof(SensorCommand.Name)] = element.Name;
            commandEntity[nameof(SensorCommand.Frame)] = element.Frame;
            commandEntity[nameof(SensorCommand.Port)] = element.Port;
        }
    }
}
