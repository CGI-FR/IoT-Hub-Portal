// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class EdgeDeviceRepository : GenericRepository<EdgeDevice>, IEdgeDeviceRepository
    {
        public EdgeDeviceRepository(PortalDbContext context) : base(context)
        {
        }

        public override Task<EdgeDevice?> GetByIdAsync(object id)
        {
            return this.context.Set<EdgeDevice>()
                .Include(device => device.Tags)
                .Where(device => device.Id.Equals(id.ToString()))
                .FirstOrDefaultAsync();
        }
    }
}
