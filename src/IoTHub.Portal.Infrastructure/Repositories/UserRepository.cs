// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class UserRepository : IUserRepository
    {
        private readonly PortalDbContext context;

        public UserRepository(PortalDbContext context)
        {
            this.context = context;
        }

        public Task<User[]> GetAllAsync()
        {
            return context.Users
                .ToArrayAsync();
        }

        public Task<User?> GetByIdAsync(string userId)
        {
            return context.Users
                .Include(r => r.AccessControls)
                .ThenInclude(ac => ac.Role)
                .FirstOrDefaultAsync(r => r.Id == userId);
        }
    }
}
