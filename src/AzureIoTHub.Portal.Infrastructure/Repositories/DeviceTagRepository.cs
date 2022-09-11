// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using AzureIoTHub.Portal.Domain.Repositories;
    using Domain.Entities;

    public class DeviceTagRepository : GenericRepository<DeviceTag>, IDeviceTagRepository
    {
        public DeviceTagRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<bool> DeviceTagExists(string deviceTagId)
        {
            return await this.context.DeviceTags.FindAsync(deviceTagId) != null;
        }

        public async Task<DeviceTag> CreateDeviceTag(DeviceTag deviceTag)
        {
            var deviceTagEntity = await this.context.DeviceTags.AddAsync(deviceTag);
            return deviceTagEntity.Entity;
        }

        public async Task<DeviceTag> UpdateDeviceTag(DeviceTag deviceTag)
        {
            var deviceTagEntity = this.context.Update(deviceTag);
            return deviceTagEntity.Entity;

        }

        public async Task<DeviceTag> CreateOrUpdateTag(DeviceTag deviceTag)
        {
            return await DeviceTagExists(deviceTag.Id) ? await UpdateDeviceTag(deviceTag) : await CreateDeviceTag(deviceTag);
        }
    }
}
