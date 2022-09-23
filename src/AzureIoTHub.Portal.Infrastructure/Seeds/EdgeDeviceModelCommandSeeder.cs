// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Seeds
{
    using Azure.Data.Tables;
    using Domain;
    using Domain.Entities;
    using Factories;
    using Microsoft.EntityFrameworkCore;

    internal static class EdgeDeviceModelCommandSeeder
    {
        public static async Task MigrateEdgeDeviceModelCommands(this PortalDbContext ctx, ConfigHandler config)
        {
            var table = new TableClientFactory(config.StorageAccountConnectionString)
                                .GetEdgeModuleCommands();

            var set = ctx.Set<EdgeDeviceModelCommand>();

            foreach (var item in table.Query<TableEntity>().ToArray())
            {
                if (await set.AnyAsync(c => c.Id == item.RowKey))
                    continue;

#pragma warning disable CS8629 // Nullable value type may be null.
                _ = await set.AddAsync(new EdgeDeviceModelCommand
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleName = item.RowKey.Split("-")[0].ToString(),
                    Name = item[nameof(EdgeDeviceModelCommand.Name)].ToString(),
                    EdgeDeviceModelId = item.PartitionKey
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
