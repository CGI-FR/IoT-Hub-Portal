// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class MenuEntryRepository : GenericRepository<MenuEntry>, IMenuEntryRepository
    {
        public MenuEntryRepository(PortalDbContext context) : base(context)
        {
        }

        public async Task<MenuEntry?> GetByNameAsync(string name)
        {
            return await this.context.Set<MenuEntry>()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
