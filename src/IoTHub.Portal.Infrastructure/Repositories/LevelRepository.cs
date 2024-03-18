
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Repositories;
    using Domain.Entities;

    public class LevelRepository : GenericRepository<Level>, ILevelRepository
    {
        public LevelRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
