// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    public class AccessControlRepository : GenericRepository<AccessControl>, IAccessControlRepository
    {
        public AccessControlRepository(PortalDbContext context) : base(context) { }

        public async Task<PaginatedResult<AccessControl>> GetPaginatedListWithDetailsAsync(int pageNumber, int pageSize, string[]? orderBy = null, Expression<Func<AccessControl, bool>>? expression = null, CancellationToken cancellationToken = default)
        {
            return await GetPaginatedListAsync(pageNumber, pageSize, orderBy, expression, cancellationToken, includes: ac => ac.Role);
        }
    }
}
