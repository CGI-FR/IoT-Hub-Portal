// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
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
                SupportLoRaFeatures = true,
                UseOTAA = bool.Parse(entity[nameof(LoRaDeviceModel.UseOTAA)]?.ToString() ?? "true"),
                PreferredWindow = int.TryParse(entity[nameof(LoRaDeviceBase.PreferredWindow)]?.ToString(), out int intResult) ? intResult : null,
                Supports32BitFCnt = bool.TryParse(entity[nameof(LoRaDeviceBase.Supports32BitFCnt)]?.ToString(), out bool boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(entity[nameof(LoRaDeviceBase.ABPRelaxMode)]?.ToString(), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(entity[nameof(LoRaDeviceBase.KeepAliveTimeout)]?.ToString(), out intResult) ? intResult : null,
                Deduplication = entity[nameof(LoRaDeviceBase.Deduplication)]?.ToString(),
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
                this.AddOptionnalProperties(entity, nameof(LoRaDeviceModel.AppEUI), model.AppEUI, desiredProperties);
            }

            this.AddOptionnalProperties(entity, nameof(LoRaDeviceModel.SensorDecoder), model.SensorDecoder, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Supports32BitFCnt), model.Supports32BitFCnt, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.ABPRelaxMode), model.ABPRelaxMode, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.KeepAliveTimeout), model.KeepAliveTimeout, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.PreferredWindow), model.PreferredWindow, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Downlink), model.Downlink, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.Deduplication), model.Deduplication, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntDownStart), model.FCntDownStart, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntResetCounter), model.FCntResetCounter, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.FCntUpStart), model.FCntUpStart, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RX1DROffset), model.RX1DROffset, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RX2DataRate), model.RX2DataRate, desiredProperties);
            this.AddOptionnalProperties(entity, nameof(LoRaDeviceBase.RXDelay), model.RXDelay, desiredProperties);

            return desiredProperties;
        }

        private void AddOptionnalProperties(TableEntity entity, string propertyName, object propertyValue, Dictionary<string, object> desiredProperties)
        {
            entity[propertyName] = propertyValue;
            desiredProperties.Add($"properties.desired.{propertyName}", propertyValue);
        }
    }
}
