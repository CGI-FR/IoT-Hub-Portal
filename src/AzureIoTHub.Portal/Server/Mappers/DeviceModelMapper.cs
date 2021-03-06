// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Models.v10;
    using System;
    using System.Collections.Generic;

    public class DeviceModelMapper : IDeviceModelMapper<DeviceModel, DeviceModel>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public DeviceModelMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceModel CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(DeviceModel.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(DeviceModel.Name)]?.ToString(),
                Description = entity[nameof(DeviceModel.Description)]?.ToString(),
            };
        }

        public DeviceModel CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(DeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(DeviceModel.Name)]?.ToString(),
                Description = entity[nameof(DeviceModel.Description)]?.ToString()
            };
        }

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, DeviceModel model)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            entity[nameof(DeviceModel.Name)] = model.Name;
            entity[nameof(DeviceModel.Description)] = model.Description;
            entity[nameof(DeviceModel.IsBuiltin)] = model.IsBuiltin;
            entity[nameof(DeviceModel.SupportLoRaFeatures)] = false;

            return new Dictionary<string, object>();
        }
    }
}
