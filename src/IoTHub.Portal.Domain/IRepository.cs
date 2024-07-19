// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Base;
    using Portal.Shared.Models.v10;

    public interface IRepository<T> where T : EntityBase
    {
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes);

        Task InsertAsync(T obj);

        void Update(T obj);

        void Delete(object id);

        Task<PaginatedResult<T>> GetPaginatedListAsync(int pageNumber, int pageSize, string[]? orderBy = null, Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);

        Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
    }
}
