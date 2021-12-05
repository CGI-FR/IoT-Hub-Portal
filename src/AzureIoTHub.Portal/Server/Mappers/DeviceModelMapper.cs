// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Shared.Models;

    public class DeviceModelMapper : IDeviceModelMapper
    {
        private readonly ISensorImageManager sensorImageManager;

        public DeviceModelMapper(ISensorImageManager sensorImageManager)
        {
            this.sensorImageManager = sensorImageManager;
        }

        public SensorModel CreateDeviceModel(TableEntity entity)
        {
            return new SensorModel
            {
                ModelId = entity.RowKey,
                ImageUrl = this.sensorImageManager.ComputeImageUri(entity.RowKey).ToString(),
                Name = entity[nameof(SensorModel.Name)]?.ToString(),
                Description = entity[nameof(SensorModel.Description)]?.ToString(),
                AppEUI = entity[nameof(SensorModel.AppEUI)]?.ToString()
            };
        }

        public void UpdateTableEntity(TableEntity entity, SensorModel model)
        {
            entity[nameof(SensorModel.Name)] = model.Name;
            entity[nameof(SensorModel.Description)] = model.Description;
            entity[nameof(SensorModel.AppEUI)] = model.AppEUI;
        }
    }
}
