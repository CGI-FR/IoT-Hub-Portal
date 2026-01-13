// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class LayerRepository : GenericRepository<Layer>, ILayerRepository
    {
        public LayerRepository(PortalDbContext context) : base(context)
        {
        }
        public async Task<Layer?> GetByNameAsync(string layerId)
        {
            return await this.Context.Set<Layer>()
                             .FirstOrDefaultAsync(layer => layer.Id == layerId);
        }
    }
}
