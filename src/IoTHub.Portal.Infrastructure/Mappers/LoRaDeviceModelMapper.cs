// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    public class LoRaDeviceModelMapper : IDeviceModelMapper<DeviceModelDto, LoRaDeviceModelDto>
    {
        public DeviceModelDto CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)]?.ToString() ?? "false"),
                Image = entity[nameof(DeviceModelDto.Image)]?.ToString(),
                Name = entity[nameof(LoRaDeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModelDto.Description)]?.ToString(),
            };
        }

        public LoRaDeviceModelDto CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new LoRaDeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                Image = entity[nameof(DeviceModelDto.Image)]?.ToString(),
                Name = entity[nameof(LoRaDeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModelDto.Description)]?.ToString(),
                SensorDecoder = entity[nameof(LoRaDeviceModelDto.SensorDecoder)]?.ToString(),
                UseOtaa = bool.Parse(entity[nameof(LoRaDeviceModelDto.UseOtaa)]?.ToString() ?? "true"),
                ClassType = Enum.TryParse<ClassType>(entity[nameof(LoRaDeviceModelDto.ClassType)]?.ToString(), out var classType) ? classType : ClassType.A,
                PreferredWindow = int.TryParse(entity[nameof(LoRaDeviceModelDto.PreferredWindow)]?.ToString(), out var intResult) ? intResult : 1,
                Supports32BitFCnt = bool.TryParse(entity[nameof(LoRaDeviceModelDto.Supports32BitFCnt)]?.ToString(), out var boolResult) ? boolResult : null,
                AbpRelaxMode = bool.TryParse(entity[nameof(LoRaDeviceModelDto.AbpRelaxMode)]?.ToString(), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(entity[nameof(LoRaDeviceModelDto.KeepAliveTimeout)]?.ToString(), out intResult) ? intResult : null,
                Deduplication = Enum.TryParse<DeduplicationMode>(entity[nameof(LoRaDeviceModelDto.Deduplication)]?.ToString(), out var deduplication) ? deduplication : DeduplicationMode.None,
                Downlink = bool.TryParse(entity[nameof(LoRaDeviceModelDto.Downlink)]?.ToString(), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntDownStart)]?.ToString(), out intResult) ? intResult : null,
                FCntResetCounter = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntResetCounter)]?.ToString(), out intResult) ? intResult : null,
                FCntUpStart = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntUpStart)]?.ToString(), out intResult) ? intResult : null,
                Rx1DrOffset = int.TryParse(entity[nameof(LoRaDeviceModelDto.Rx1DrOffset)]?.ToString(), out intResult) ? intResult : null,
                Rx2DataRate = int.TryParse(entity[nameof(LoRaDeviceModelDto.Rx2DataRate)]?.ToString(), out intResult) ? intResult : null,
                RxDelay = int.TryParse(entity[nameof(LoRaDeviceModelDto.RxDelay)]?.ToString(), out intResult) ? intResult : null
            };
        }

        public Dictionary<string, object> BuildDeviceModelDesiredProperties(LoRaDeviceModelDto model)
        {
            var desiredProperties = new Dictionary<string, object>();

            AddOptionalProperties(nameof(LoRaDeviceModelDto.SensorDecoder), model.SensorDecoder, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Supports32BitFCnt), model.Supports32BitFCnt!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.AbpRelaxMode), model.AbpRelaxMode!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.KeepAliveTimeout), model.KeepAliveTimeout!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.PreferredWindow), model.PreferredWindow, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Downlink), model.Downlink!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Deduplication), model.Deduplication.ToString(), desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntDownStart), model.FCntDownStart!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntResetCounter), model.FCntResetCounter!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntUpStart), model.FCntUpStart!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Rx1DrOffset), model.Rx1DrOffset!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Rx2DataRate), model.Rx2DataRate!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.RxDelay), model.RxDelay!, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.ClassType), model.ClassType.ToString(), desiredProperties);

            return desiredProperties;
        }

        private static void AddOptionalProperties(string propertyName, object propertyValue, Dictionary<string, object> desiredProperties)
        {
            desiredProperties.Add($"properties.desired.{propertyName}", propertyValue);
        }
    }
}
