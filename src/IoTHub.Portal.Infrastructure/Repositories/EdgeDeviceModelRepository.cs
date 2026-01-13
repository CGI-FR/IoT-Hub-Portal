// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class EdgeDeviceModelRepository : GenericRepository<EdgeDeviceModel>, IEdgeDeviceModelRepository
    {
        public EdgeDeviceModelRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<EdgeDeviceModel?> GetByNameAsync(string edgeModelDevice)
        {
            return await this.Context.Set<EdgeDeviceModel>()
                             .FirstOrDefaultAsync(edgeDeviceModel => edgeDeviceModel.Name == edgeModelDevice);
        }
    }
}
