// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Infrastructure.Managers;
    using AzureIoTHub.Portal.Models.v10;
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Application.Managers;

    public class DeviceModelMapper : IDeviceModelMapper<DeviceModelDto, DeviceModelDto>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceModelMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceModelDto CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(DeviceModelDto.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(DeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(DeviceModelDto.Description)]?.ToString(),
            };
        }

        public DeviceModelDto CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
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
