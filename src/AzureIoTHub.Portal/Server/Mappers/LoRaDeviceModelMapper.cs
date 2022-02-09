// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;

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
                SensorDecoderURL = entity[nameof(LoRaDeviceModel.SensorDecoderURL)]?.ToString(),
                SupportLoRaFeatures = true
            };
        }

        public void UpdateTableEntity(TableEntity entity, LoRaDeviceModel model)
        {
            entity[nameof(LoRaDeviceModel.Name)] = model.Name;
            entity[nameof(LoRaDeviceModel.Description)] = model.Description;
            entity[nameof(LoRaDeviceModel.IsBuiltin)] = model.IsBuiltin;
            entity[nameof(LoRaDeviceModel.AppEUI)] = model.AppEUI;
            entity[nameof(LoRaDeviceModel.SensorDecoderURL)] = model.SensorDecoderURL;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = true;
        }
    }
}
