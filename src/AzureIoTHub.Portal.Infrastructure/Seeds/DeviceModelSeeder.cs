// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Seeds
{
    using Azure.Data.Tables;
    using Domain;
    using Domain.Entities;
    using Factories;
    using Microsoft.EntityFrameworkCore;
    using Models.v10.LoRaWAN;

    internal static class DeviceModelSeeder
    {
        public static async Task MigrateDeviceModels(this PortalDbContext ctx, ConfigHandler config)
        {
            var table = new TableClientFactory(config.StorageAccountConnectionString)
                                .GetDeviceTemplates();

            var set = ctx.Set<DeviceModel>();

            foreach (var item in table.Query<TableEntity>().ToArray())
            {
                if (await set.AnyAsync(c => c.Id == item.RowKey))
                    continue;

#pragma warning disable CS8629 // Nullable value type may be null.
                _ = await set.AddAsync(new DeviceModel
                {
                    Id = item.RowKey,
                    Name = item[nameof(DeviceModel.Name)]?.ToString(),
                    Description = item[nameof(DeviceModel.Description)]?.ToString(),
                    IsBuiltin = bool.Parse(item[nameof(DeviceModel.IsBuiltin)]?.ToString() ?? "false"),
                    SupportLoRaFeatures = bool.Parse(item[nameof(DeviceModel.SupportLoRaFeatures)]?.ToString() ?? "false"),
                    ABPRelaxMode = bool.TryParse(item[nameof(DeviceModel.ABPRelaxMode)]?.ToString(), out var abpRelaxMode) ? abpRelaxMode : null,
                    Deduplication = Enum.TryParse<DeduplicationMode>(item[nameof(DeviceModel.Deduplication)]?.ToString(), out var deduplication) ? deduplication : null,
                    Downlink = bool.TryParse(item[nameof(DeviceModel.Downlink)]?.ToString(), out var downLink) ? downLink : null,
                    KeepAliveTimeout = int.TryParse(item[nameof(DeviceModel.KeepAliveTimeout)]?.ToString(), out var keepAliveTimeout) ? keepAliveTimeout : null,
                    PreferredWindow = int.TryParse(item[nameof(DeviceModel.PreferredWindow)]?.ToString(), out var preferredWindow) ? preferredWindow : null,
                    RXDelay = int.TryParse(item[nameof(DeviceModel.PreferredWindow)]?.ToString(), out var rxDelay) ? rxDelay : null,
                    UseOTAA = bool.TryParse(item[nameof(DeviceModel.UseOTAA)]?.ToString(), out var useOTAA) ? useOTAA : null,
                    AppEUI = item[nameof(DeviceModel.AppEUI)]?.ToString(),
                    SensorDecoder = item[nameof(DeviceModel.SensorDecoder)]?.ToString()
                });
#pragma warning restore CS8629 // Nullable value type may be null.

                if (config is ProductionConfigHandler)
                {
                    _ = await table.DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
            }
        }
    }
}
