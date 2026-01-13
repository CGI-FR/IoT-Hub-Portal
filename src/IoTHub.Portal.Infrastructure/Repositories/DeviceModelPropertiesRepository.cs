// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class DeviceModelPropertiesRepository : GenericRepository<DeviceModelProperty>, IDeviceModelPropertiesRepository
    {
        public DeviceModelPropertiesRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId)
        {
            return await this.Context.Set<DeviceModelProperty>()
                        .Where(x => x.ModelId == modelId)
                        .ToArrayAsync();
        }

        public async Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var modelItems = this.Context.Set<DeviceModelProperty>().Where(c => c.ModelId == modelId);

            var deviceModelProperties = items.ToList();

            foreach (var item in modelItems)
            {
                if (deviceModelProperties.Any(c => c.Id == item.Id))
                {
                    _ = this.Context.Update(item);
                    continue;
                }

                _ = this.Context.Remove(item);
            }

            foreach (var item in deviceModelProperties)
            {
                if (modelItems.Any(c => c.Id == item.Id))
                {
                    continue;
                }

                item.ModelId = modelId;

                _ = await this.Context.AddAsync(item);
            }
        }
    }
}
