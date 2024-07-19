// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {
        public GroupRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<Group?> GetByNameAsync(string groupName, params Expression<Func<Group, object>>[] includeProperties)
        {
            IQueryable<Group> query = context.Groups;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync(g => g.Name == groupName);
        }
    }
}
