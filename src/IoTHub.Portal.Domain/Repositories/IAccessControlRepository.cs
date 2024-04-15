// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v1._0;

    public interface IAccessControlRepository : IRepository<AccessControl>
    {
        Task<PaginatedResult<AccessControl>> GetPaginatedListWithDetailsAsync(int pageNumber, int pageSize, string[]? orderBy = null, Expression<Func<AccessControl, bool>>? expression = null, CancellationToken cancellationToken = default);
    }
}
