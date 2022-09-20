// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using Domain.Entities;
    using Domain.Repositories;

    public class EdgeDeviceModelCommandRepository : GenericRepository<EdgeDeviceModelCommand>, IEdgeDeviceModelCommandRepository
    {
        public EdgeDeviceModelCommandRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
