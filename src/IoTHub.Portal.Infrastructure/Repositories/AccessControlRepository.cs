// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class AccessControlRepository : IAccessControlRepository
    {
        private readonly PortalDbContext context;

        public AccessControlRepository(PortalDbContext context)
        {
            this.context = context;
        }

        public Task<AccessControl[]> GetAllAsync()
        {
            return context.AccessControls.ToArrayAsync();
        }
        public Task<AccessControl> GetByIdAsync(string accessControlId)
        {
            return context.AccessControls.FirstOrDefaultAsync(r => r.Id == accessControlId);
        }
    }
}
