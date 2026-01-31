// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public class DeviceModelMapper : IDeviceModelMapper<DeviceModelDto, DeviceModelDto>
    {
        public DeviceModelDto CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(DeviceModelDto.SupportLoRaFeatures)]?.ToString() ?? "false"),
                Image = entity[nameof(DeviceModelDto.Image)]?.ToString(),
                Name = entity[nameof(DeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(DeviceModelDto.Description)]?.ToString(),
            };
        }

        public DeviceModelDto CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                Image = entity[nameof(DeviceModelDto.Image)]?.ToString(),
                Name = entity[nameof(DeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(DeviceModelDto.Description)]?.ToString()
            };
        }

        public Dictionary<string, object> BuildDeviceModelDesiredProperties(DeviceModelDto model)
        {
            return new Dictionary<string, object>();
        }
    }
}
