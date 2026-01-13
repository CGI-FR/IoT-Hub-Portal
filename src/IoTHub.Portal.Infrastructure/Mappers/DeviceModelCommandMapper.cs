// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public class DeviceModelCommandMapper : IDeviceModelCommandMapper
    {
        /// <summary>
        /// Gets the device model command.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The device model comamnd.</returns>
        public DeviceModelCommandDto GetDeviceModelCommand(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DeviceModelCommandDto
            {
                Name = entity.RowKey,
                Frame = entity[nameof(DeviceModelCommandDto.Frame)].ToString(),
                Port = int.Parse(entity[nameof(DeviceModelCommandDto.Port)].ToString()!, CultureInfo.InvariantCulture),
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelCommandDto.IsBuiltin)]?.ToString() ?? "false"),
                Confirmed = bool.Parse(entity[nameof(DeviceModelCommandDto.Confirmed)]?.ToString() ?? "false"),
            };
        }

        /// <summary>
        /// Updates the table entity.
        /// </summary>
        /// <param name="commandEntity">The command entity.</param>
        /// <param name="element">The element.</param>
        public void UpdateTableEntity(TableEntity commandEntity, DeviceModelCommandDto element)
        {
            ArgumentNullException.ThrowIfNull(commandEntity);
            ArgumentNullException.ThrowIfNull(element);

            commandEntity[nameof(DeviceModelCommandDto.Frame)] = element.Frame;
            commandEntity[nameof(DeviceModelCommandDto.Port)] = element.Port;
            commandEntity[nameof(DeviceModelCommandDto.IsBuiltin)] = element.IsBuiltin;
            commandEntity[nameof(DeviceModelCommandDto.Confirmed)] = element.Confirmed;
        }
    }
}
