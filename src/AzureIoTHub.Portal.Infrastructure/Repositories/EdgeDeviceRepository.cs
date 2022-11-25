// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;

    public class EdgeDeviceRepository : GenericRepository<EdgeDevice>, IEdgeDeviceRepository
    {
        public EdgeDeviceRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
