// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByNameAsync(string userName, params Expression<Func<User, object>>[] includeProperties)
        {
            IQueryable<User> query = context.Users;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync(u => u.Name == userName);
        }
    }
}
