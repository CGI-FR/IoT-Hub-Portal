// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class GroupRepository : IGroupRepository
    {
        private readonly PortalDbContext context;

        public GroupRepository(PortalDbContext context)
        {
            this.context = context;
        }

        public Task<Group[]> GetAllAsync()
        {
            return context.Groups.ToArrayAsync();
        }
        public Task<Group> GetByIdAsync(string groupId)
        {
            return context.Groups.FirstOrDefaultAsync(r => r.Id == groupId);
        }
        public Task<Group> CreateAsync(Group group)
        {
            _ = this.context.Groups.Add(group);
            return Task.FromResult(group);
        }
    }
}
