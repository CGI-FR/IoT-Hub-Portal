// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class RoleRepository : IRoleRepository
    {
        private readonly PortalDbContext context;

        public RoleRepository(PortalDbContext context)
        {
            this.context = context;
        }

        public Task<Role[]> GetAllAsync()
        {
            return context.Roles.ToArrayAsync();
        }
        public Task<Role> GetByIdAsync(string roleId)
        {
            return context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        }
    }
}
