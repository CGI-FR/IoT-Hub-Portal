// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using Domain;
    using Domain.Base;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Shared.Models.v10;
    using System.Linq.Dynamic.Core;

    public class GenericRepository<T> : IRepository<T> where T : EntityBase
    {
        protected readonly PortalDbContext context;

        public GenericRepository(PortalDbContext context)
        {
            this.context = context;
        }

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = this.context.Set<T>();

            query = includes.Aggregate(query, (current, include) => current.Include(include));

            return query.ToList<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = this.context.Set<T>();
            query = includes.Aggregate(query, (current, include) => current.Include(include));
            if (expression != null) query = query.Where(expression);

            return await query.ToDynamicListAsync<T>(cancellationToken: cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = this.context.Set<T>();

            query = query.Where(entity => entity.Id.Equals(id));

            query = includes.Aggregate(query, (current, include) => current.Include(include));

            return await query.FirstOrDefaultAsync();
        }

        public async Task InsertAsync(T obj)
        {
            _ = await this.context.Set<T>()
                .AddAsync(obj);
        }

        public void Update(T obj)
        {
            _ = this.context.Set<T>().Attach(obj);
            this.context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(object id)
        {
            var existing = this.context
                    .Set<T>()
                    .Find(id);

            _ = this.context.Set<T>().Remove(existing!);
        }

        public async Task<PaginatedResult<T>> GetPaginatedListAsync(
            int pageNumber,
            int pageSize,
            string[]? orderBy = null,
            Expression<Func<T, bool>>? expression = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = this.context.Set<T>();

            query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (expression != null) query = query.Where(expression);

            var ordering = orderBy?.Any() == true ? string.Join(",", orderBy) : null;

            query = !string.IsNullOrWhiteSpace(ordering) ? query.OrderBy(ordering) : query.OrderBy(a => a.Id);

            var count = await query
                .AsNoTracking()
                .CountAsync(cancellationToken: cancellationToken);

            var items = await query
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToDynamicListAsync<T>(cancellationToken: cancellationToken);

            return new PaginatedResult<T>(items, count, pageNumber, pageSize);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = this.context.Set<T>();
            if (expression != null) query = query.Where(expression);

            return await query
                .AsNoTracking()
                .CountAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await this.context.Set<T>().AnyAsync(expression, cancellationToken: cancellationToken);
        }
    }
}
