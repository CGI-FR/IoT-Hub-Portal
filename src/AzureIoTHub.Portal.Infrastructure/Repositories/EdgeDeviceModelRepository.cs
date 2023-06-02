// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using Domain.Entities;
    using Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class EdgeDeviceModelRepository : GenericRepository<EdgeDeviceModel>, IEdgeDeviceModelRepository
    {
        public EdgeDeviceModelRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<EdgeDeviceModel?> GetByNameAsync(string edgeModelDevice)
        {
            return await this.context.Set<EdgeDeviceModel>()
                             .FirstOrDefaultAsync(edgeDeviceModel => edgeDeviceModel.Name == edgeModelDevice);
        }
    }
}
