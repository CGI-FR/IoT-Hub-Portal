﻿// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
    using System.Collections.Generic;

    public class LoRaDeviceModelMapper : IDeviceModelMapper<DeviceModel, LoRaDeviceModel>
    {
        private readonly IDeviceModelCommandsManager deviceModelCommandsManager;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaDeviceModelMapper(IDeviceModelCommandsManager deviceModelCommandsManager, IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelCommandsManager = deviceModelCommandsManager;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceModel CreateDeviceModelListItem(TableEntity entity)
        {
            return new DeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey).ToString(),
                Name = entity[nameof(LoRaDeviceModel.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModel.Description)]?.ToString(),
            };
        }

        public LoRaDeviceModel CreateDeviceModel(TableEntity entity)
        {
            return new LoRaDeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey).ToString(),
                Name = entity[nameof(LoRaDeviceModel.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModel.Description)]?.ToString(),
                AppEUI = entity[nameof(LoRaDeviceModel.AppEUI)]?.ToString(),
                SensorDecoder = entity[nameof(LoRaDeviceModel.SensorDecoder)]?.ToString(),
                SupportLoRaFeatures = true
            };
        }

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, LoRaDeviceModel model)
        {
            entity[nameof(LoRaDeviceModel.Name)] = model.Name;
            entity[nameof(LoRaDeviceModel.Description)] = model.Description;
            entity[nameof(LoRaDeviceModel.IsBuiltin)] = model.IsBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = model.SupportLoRaFeatures;

            entity[nameof(LoRaDeviceModel.AppEUI)] = model.AppEUI;
            entity[nameof(LoRaDeviceModel.SensorDecoder)] = model.SensorDecoder;

            var desiredProperties = new Dictionary<string, object>();

            desiredProperties.Add($"properties.desired.{nameof(LoRaDeviceModel.AppEUI)}", model.AppEUI);
            desiredProperties.Add($"properties.desired.{nameof(LoRaDeviceModel.SensorDecoder)}", model.SensorDecoder);

            return desiredProperties;
        }
    }
}
