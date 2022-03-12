// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.v10.DeviceModel;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN.LoRaDeviceModel;
    using System.Collections.Generic;
    using System;

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
                SupportLoRaFeatures = true,
                UseOTAA = bool.Parse(entity[nameof(LoRaDeviceModel.UseOTAA)]?.ToString() ?? "true"),
                PreferredWindow = int.TryParse(entity[nameof(LoRaDeviceBase.PreferredWindow)]?.ToString(), out var intResult) ? intResult : 1,
                Supports32BitFCnt = bool.TryParse(entity[nameof(LoRaDeviceBase.Supports32BitFCnt)]?.ToString(), out var boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(entity[nameof(LoRaDeviceBase.ABPRelaxMode)]?.ToString(), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(entity[nameof(LoRaDeviceBase.KeepAliveTimeout)]?.ToString(), out intResult) ? intResult : null,
                Deduplication = Enum.TryParse<DeduplicationMode>(entity[nameof(LoRaDeviceBase.Deduplication)]?.ToString(), out var deduplication) ? deduplication : DeduplicationMode.None,
                Downlink = bool.TryParse(entity[nameof(LoRaDeviceBase.Downlink)]?.ToString(), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(entity[nameof(LoRaDeviceBase.FCntDownStart)]?.ToString(), out intResult) ? intResult : null,
                FCntResetCounter = int.TryParse(entity[nameof(LoRaDeviceBase.FCntResetCounter)]?.ToString(), out intResult) ? intResult : null,
                FCntUpStart = int.TryParse(entity[nameof(LoRaDeviceBase.FCntUpStart)]?.ToString(), out intResult) ? intResult : null,
                RX1DROffset = int.TryParse(entity[nameof(LoRaDeviceBase.RX1DROffset)]?.ToString(), out intResult) ? intResult : null,
                RX2DataRate = int.TryParse(entity[nameof(LoRaDeviceBase.RX2DataRate)]?.ToString(), out intResult) ? intResult : null,
                RXDelay = int.TryParse(entity[nameof(LoRaDeviceBase.RXDelay)]?.ToString(), out intResult) ? intResult : null
            };
        }

        public Dictionary<string, object> UpdateTableEntity(TableEntity entity, LoRaDeviceModel model)
        {
            entity[nameof(LoRaDeviceModel.Name)] = model.Name;
            entity[nameof(LoRaDeviceModel.Description)] = model.Description;
            entity[nameof(LoRaDeviceModel.IsBuiltin)] = model.IsBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = model.SupportLoRaFeatures;
            entity[nameof(LoRaDeviceModel.UseOTAA)] = model.UseOTAA;

            var desiredProperties = new Dictionary<string, object>();

            if (model.UseOTAA)
            {
                AddOptionnalProperties(entity, nameof(LoRaDeviceModel.AppEUI), model.AppEUI, desiredProperties);
            }

            AddOptionnalProperties(entity, nameof(LoRaDeviceModel.SensorDecoder), model.SensorDecoder, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Supports32BitFCnt), model.Supports32BitFCnt, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.ABPRelaxMode), model.ABPRelaxMode, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.KeepAliveTimeout), model.KeepAliveTimeout, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.PreferredWindow), model.PreferredWindow, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Downlink), model.Downlink, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Deduplication), model.Deduplication.ToString(), desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntDownStart), model.FCntDownStart, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntResetCounter), model.FCntResetCounter, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntUpStart), model.FCntUpStart, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RX1DROffset), model.RX1DROffset, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RX2DataRate), model.RX2DataRate, desiredProperties);
            AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RXDelay), model.RXDelay, desiredProperties);

            return desiredProperties;
        }

        private static void AddOptionnalProperties(TableEntity entity, string propertyName, object propertyValue, Dictionary<string, object> desiredProperties)
        {
            entity[propertyName] = propertyValue;
            desiredProperties.Add($"properties.desired.{propertyName}", propertyValue);
        }
    }
}
