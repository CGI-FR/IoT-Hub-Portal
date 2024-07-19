// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(PortalDbContext context) : base(context)
        {
        }
        public async Task<Role?> GetByNameAsync(string roleName, params Expression<Func<Role, object>>[] includeProperties)
        {
            IQueryable<Role> query = context.Roles;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync(r => r.Name == roleName);
        }
    }
}
