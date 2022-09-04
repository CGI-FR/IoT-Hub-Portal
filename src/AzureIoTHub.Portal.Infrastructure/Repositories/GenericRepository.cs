// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Base;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class GenericRepository<T> : IRepository<T> where T : EntityBase
    {
        private readonly DbContext context;

        public GenericRepository(DbContext context)
        {
            this.context = context;
        }

        public IEnumerable<T> GetAll()
        {
            return this.context.Set<T>()
                                .ToList<T>();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            var t = await this.context.Set<T>().FindAsync(id);
            return t;
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

            _ = this.context.Set<T>().Remove(existing);
        }
    }
}
