// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

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
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(LoRaDeviceModel.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModel.Description)]?.ToString(),
            };
        }

        public LoRaDeviceModel CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new LoRaDeviceModel
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(LoRaDeviceModel.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModel.Description)]?.ToString(),
                SensorDecoder = entity[nameof(LoRaDeviceModel.SensorDecoder)]?.ToString(),
                UseOTAA = bool.Parse(entity[nameof(LoRaDeviceModel.UseOTAA)]?.ToString() ?? "true"),
                PreferredWindow = int.TryParse(entity[nameof(LoRaDeviceModel.PreferredWindow)]?.ToString(), out var intResult) ? intResult : 1,
                Supports32BitFCnt = bool.TryParse(entity[nameof(LoRaDeviceModel.Supports32BitFCnt)]?.ToString(), out var boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(entity[nameof(LoRaDeviceModel.ABPRelaxMode)]?.ToString(), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(entity[nameof(LoRaDeviceModel.KeepAliveTimeout)]?.ToString(), out intResult) ? intResult : null,
                Deduplication = Enum.TryParse<DeduplicationMode>(entity[nameof(LoRaDeviceModel.Deduplication)]?.ToString(), out var deduplication) ? deduplication : DeduplicationMode.None,
                Downlink = bool.TryParse(entity[nameof(LoRaDeviceModel.Downlink)]?.ToString(), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(entity[nameof(LoRaDeviceModel.FCntDownStart)]?.ToString(), out intResult) ? intResult : null,
                FCntResetCounter = int.TryParse(entity[nameof(LoRaDeviceModel.FCntResetCounter)]?.ToString(), out intResult) ? intResult : null,
                FCntUpStart = int.TryParse(entity[nameof(LoRaDeviceModel.FCntUpStart)]?.ToString(), out intResult) ? intResult : null,
                RX1DROffset = int.TryParse(entity[nameof(LoRaDeviceModel.RX1DROffset)]?.ToString(), out intResult) ? intResult : null,
                RX2DataRate = int.TryParse(entity[nameof(LoRaDeviceModel.RX2DataRate)]?.ToString(), out intResult) ? intResult : null,
                RXDelay = int.TryParse(entity[nameof(LoRaDeviceModel.RXDelay)]?.ToString(), out intResult) ? intResult : null
            };
        }

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, LoRaDeviceModel model)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            entity[nameof(LoRaDeviceModel.Name)] = model.Name;
            entity[nameof(LoRaDeviceModel.Description)] = model.Description;
            entity[nameof(LoRaDeviceModel.IsBuiltin)] = model.IsBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = model.SupportLoRaFeatures;
            entity[nameof(LoRaDeviceModel.UseOTAA)] = model.UseOTAA;

            var desiredProperties = new Dictionary<string, object>();

            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.SensorDecoder), model.SensorDecoder, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.Supports32BitFCnt), model.Supports32BitFCnt, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.ABPRelaxMode), model.ABPRelaxMode, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.KeepAliveTimeout), model.KeepAliveTimeout, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.PreferredWindow), model.PreferredWindow, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.Downlink), model.Downlink, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.Deduplication), model.Deduplication.ToString(), desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.FCntDownStart), model.FCntDownStart, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.FCntResetCounter), model.FCntResetCounter, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.FCntUpStart), model.FCntUpStart, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.RX1DROffset), model.RX1DROffset, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.RX2DataRate), model.RX2DataRate, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.RXDelay), model.RXDelay, desiredProperties);

            return desiredProperties;
        }

        private static void AddOptionnalProperties(TableEntity entity, string propertyName, object propertyValue, Dictionary<string, object> desiredProperties)
        {
            entity[propertyName] = propertyValue;
            desiredProperties.Add($"properties.desired.{propertyName}", propertyValue);
        }
    }
}
