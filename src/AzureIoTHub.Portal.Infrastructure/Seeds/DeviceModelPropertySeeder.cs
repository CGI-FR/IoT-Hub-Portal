// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Seeds
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Models;
    using AzureIoTHub.Portal.Infrastructure.Factories;

    internal static class DeviceModelPropertySeeder
    {
        public static async Task MigrateDeviceModelProperties(this PortalDbContext ctx, ConfigHandler config)
        {
            var table = new TableClientFactory(config.StorageAccountConnectionString)
                                .GetDeviceTemplateProperties();

            var set = ctx.Set<DeviceModelProperty>();

            foreach (var item in table.Query<TableEntity>()
                    .Where(item => set.Any(c => c.Id == item.RowKey))
                    .ToArray())
            {

#pragma warning disable CS8629 // Nullable value type may be null.
                _ = await set.AddAsync(new DeviceModelProperty
                {
                    Id = item.RowKey,
                    ModelId = item.PartitionKey,
                    Name = item.GetString(nameof(DeviceModelProperty.Name)),
                    DisplayName = item.GetString(nameof(DeviceModelProperty.DisplayName)),
                    PropertyType = Enum.Parse<DevicePropertyType>(item.GetString(nameof(DeviceModelProperty.PropertyType))),
                    Order = item.GetInt32(nameof(DeviceModelProperty.Order)) ?? 0,
                    IsWritable = item.GetBoolean(nameof(DeviceModelProperty.IsWritable)).Value
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
