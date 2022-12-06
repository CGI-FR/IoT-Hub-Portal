// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Mappers
{
    using System;
    using System.Collections.Generic;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Application.Managers;
    using Managers;
    using Models.v10;
    using Models.v10.LoRaWAN;

    public class LoRaDeviceModelMapper : IDeviceModelMapper<DeviceModelDto, LoRaDeviceModelDto>
    {
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public LoRaDeviceModelMapper(IDeviceModelImageManager deviceModelImageManager)
        {
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public DeviceModelDto CreateDeviceModelListItem(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new DeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                SupportLoRaFeatures = bool.Parse(entity[nameof(LoRaDeviceModelDto.SupportLoRaFeatures)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(LoRaDeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModelDto.Description)]?.ToString(),
            };
        }

        public LoRaDeviceModelDto CreateDeviceModel(TableEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new LoRaDeviceModelDto
            {
                ModelId = entity.RowKey,
                IsBuiltin = bool.Parse(entity[nameof(LoRaDeviceModelDto.IsBuiltin)]?.ToString() ?? "false"),
                ImageUrl = this.deviceModelImageManager.ComputeImageUri(entity.RowKey),
                Name = entity[nameof(LoRaDeviceModelDto.Name)]?.ToString(),
                Description = entity[nameof(LoRaDeviceModelDto.Description)]?.ToString(),
                SensorDecoder = entity[nameof(LoRaDeviceModelDto.SensorDecoder)]?.ToString(),
                UseOTAA = bool.Parse(entity[nameof(LoRaDeviceModelDto.UseOTAA)]?.ToString() ?? "true"),
                ClassType = Enum.TryParse<ClassType>(entity[nameof(LoRaDeviceModelDto.ClassType)]?.ToString(), out var classType) ? classType : ClassType.A,
                PreferredWindow = int.TryParse(entity[nameof(LoRaDeviceModelDto.PreferredWindow)]?.ToString(), out var intResult) ? intResult : 1,
                Supports32BitFCnt = bool.TryParse(entity[nameof(LoRaDeviceModelDto.Supports32BitFCnt)]?.ToString(), out var boolResult) ? boolResult : null,
                ABPRelaxMode = bool.TryParse(entity[nameof(LoRaDeviceModelDto.ABPRelaxMode)]?.ToString(), out boolResult) ? boolResult : null,
                KeepAliveTimeout = int.TryParse(entity[nameof(LoRaDeviceModelDto.KeepAliveTimeout)]?.ToString(), out intResult) ? intResult : null,
                Deduplication = Enum.TryParse<DeduplicationMode>(entity[nameof(LoRaDeviceModelDto.Deduplication)]?.ToString(), out var deduplication) ? deduplication : DeduplicationMode.None,
                Downlink = bool.TryParse(entity[nameof(LoRaDeviceModelDto.Downlink)]?.ToString(), out boolResult) ? boolResult : null,
                FCntDownStart = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntDownStart)]?.ToString(), out intResult) ? intResult : null,
                FCntResetCounter = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntResetCounter)]?.ToString(), out intResult) ? intResult : null,
                FCntUpStart = int.TryParse(entity[nameof(LoRaDeviceModelDto.FCntUpStart)]?.ToString(), out intResult) ? intResult : null,
                RX1DROffset = int.TryParse(entity[nameof(LoRaDeviceModelDto.RX1DROffset)]?.ToString(), out intResult) ? intResult : null,
                RX2DataRate = int.TryParse(entity[nameof(LoRaDeviceModelDto.RX2DataRate)]?.ToString(), out intResult) ? intResult : null,
                RXDelay = int.TryParse(entity[nameof(LoRaDeviceModelDto.RXDelay)]?.ToString(), out intResult) ? intResult : null
            };
        }

        public Dictionary<string, object> BuildDeviceModelDesiredProperties(LoRaDeviceModelDto modelDto)
        {
            var desiredProperties = new Dictionary<string, object>();

            AddOptionalProperties(nameof(LoRaDeviceModelDto.SensorDecoder), modelDto.SensorDecoder, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Supports32BitFCnt), modelDto.Supports32BitFCnt, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.ABPRelaxMode), modelDto.ABPRelaxMode, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.KeepAliveTimeout), modelDto.KeepAliveTimeout, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.PreferredWindow), modelDto.PreferredWindow, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Downlink), modelDto.Downlink, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.Deduplication), modelDto.Deduplication.ToString(), desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntDownStart), modelDto.FCntDownStart, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntResetCounter), modelDto.FCntResetCounter, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.FCntUpStart), modelDto.FCntUpStart, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.RX1DROffset), modelDto.RX1DROffset, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.RX2DataRate), modelDto.RX2DataRate, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.RXDelay), modelDto.RXDelay, desiredProperties);
            AddOptionalProperties(nameof(LoRaDeviceModelDto.ClassType), modelDto.ClassType.ToString(), desiredProperties);

            return desiredProperties;
        }

        private static void AddOptionalProperties(string propertyName, object propertyValue, Dictionary<string, object> desiredProperties)
        {
            desiredProperties.Add($"properties.desired.{propertyName}", propertyValue);
        }
    }
}
