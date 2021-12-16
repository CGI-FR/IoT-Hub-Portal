// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models;

    public class DeviceModelMapper : IDeviceModelMapper
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceModelCommandsManager deviceModelCommandsManager;

        public DeviceModelMapper(IDeviceModelImageManager deviceModelImageManager, IDeviceModelCommandsManager deviceModelCommandsManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceModelCommandsManager = deviceModelCommandsManager;
        }

        public DeviceModel CreateDeviceModel(TableEntity entity)
        {
            return new DeviceModel
            {
                ModelId = entity.RowKey,
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey).ToString(),
                Name = entity[nameof(DeviceModel.Name)]?.ToString(),
                Description = entity[nameof(DeviceModel.Description)]?.ToString(),
                AppEUI = entity[nameof(DeviceModel.AppEUI)]?.ToString(),
                SensorDecoderURL = entity[nameof(DeviceModel.SensorDecoderURL)]?.ToString(),
                Commands = this.deviceModelCommandsManager.RetrieveDeviceModelCommands(entity.RowKey)
            };
        }

        public void UpdateTableEntity(TableEntity entity, DeviceModel model)
        {
            entity[nameof(DeviceModel.Name)] = model.Name;
            entity[nameof(DeviceModel.Description)] = model.Description;
            entity[nameof(DeviceModel.AppEUI)] = model.AppEUI;
            entity[nameof(DeviceModel.SensorDecoderURL)] = model.SensorDecoderURL;
        }
    }
}
