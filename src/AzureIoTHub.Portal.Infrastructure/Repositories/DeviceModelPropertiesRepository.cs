// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class DeviceModelPropertiesRepository : GenericRepository<DeviceModelProperty>, IDeviceModelPropertiesRepository
    {
        public DeviceModelPropertiesRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId)
        {
            return await this.context.Set<DeviceModelProperty>()
                        .Where(x => x.ModelId == modelId)
                        .ToArrayAsync();
        }

        public async Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            var modelItems = this.context.Set<DeviceModelProperty>().Where(c => c.ModelId == modelId);

            foreach (var item in modelItems)
            {
                if (items.Any(c => c.Id == item.Id))
                {
                    _ = this.context.Update(item);
                    continue;
                }

                _ = this.context.Remove(item);
            }

            foreach (var item in items)
            {
                if (modelItems.Any(c => c.Id == item.Id))
                {
                    continue;
                }

                item.ModelId = modelId;

                _ = await this.context.AddAsync(item);
            }
        }
    }
}
