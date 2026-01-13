// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class DeviceModelRepository : GenericRepository<DeviceModel>, IDeviceModelRepository
    {
        public DeviceModelRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<DeviceModel?> GetByNameAsync(string modelName)
        {
            return await this.Context.Set<DeviceModel>()
                             .FirstOrDefaultAsync(deviceModel => deviceModel.Name == modelName);
        }
    }
}
