// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using AzureIoTHub.Portal.Domain.Repositories;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class LorawanDeviceRepository : GenericRepository<LorawanDevice>, ILorawanDeviceRepository
    {
        public LorawanDeviceRepository(PortalDbContext context) : base(context)
        {
        }

        public override Task<LorawanDevice?> GetByIdAsync(object id)
        {
            return this.context.Set<LorawanDevice>()
                .Include(device => device.Tags)
                .Where(device => device.Id.Equals(id.ToString()))
                .FirstOrDefaultAsync();
        }
    }
}
