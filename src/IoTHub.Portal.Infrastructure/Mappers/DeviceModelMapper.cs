// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    using Azure.Data.Tables;
    using IoTHub.Portal.Models.v10;
    using System;
    using System.Collections.Generic;

    public class DeviceModelMapper : IDeviceModelMapper<DeviceModelDto, DeviceModelDto>
    {
        public DeviceModelMapper()
        {
        }

        public DeviceModelDto CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(DeviceModelDto.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = entity[nameof(DeviceModelDto.ImageUrl)]?.ToString(),
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
                ImageUrl = entity[nameof(DeviceModelDto.ImageUrl)]?.ToString(),
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
