// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Seeds
{
    using System.Globalization;
    using Azure.Data.Tables;
    using Domain;
    using Domain.Entities;
    using Factories;
    using Microsoft.EntityFrameworkCore;

    internal static class DeviceModelCommandSeeder
    {
        public static async Task MigrateDeviceModelCommands(this PortalDbContext ctx, ConfigHandler config)
        {
            var table = new TableClientFactory(config.StorageAccountConnectionString)
                                .GetDeviceCommands();

            var set = ctx.Set<DeviceModelCommand>();

            foreach (var item in table.Query<TableEntity>().ToArray())
            {
                if (await set.AnyAsync(c => c.Id == item.RowKey))
                    continue;

#pragma warning disable CS8629 // Nullable value type may be null.
                _ = await set.AddAsync(new DeviceModelCommand
                {
                    Id = item.RowKey,
                    Frame = item[nameof(DeviceModelCommand.Frame)].ToString(),
                    Port = int.Parse(item[nameof(DeviceModelCommand.Port)].ToString(), CultureInfo.InvariantCulture),
                    IsBuiltin = bool.Parse(item[nameof(DeviceModelCommand.IsBuiltin)]?.ToString() ?? "false"),
                    Confirmed = bool.Parse(item[nameof(DeviceModelCommand.Confirmed)]?.ToString() ?? "false"),
                    DeviceModelId = item.PartitionKey
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
