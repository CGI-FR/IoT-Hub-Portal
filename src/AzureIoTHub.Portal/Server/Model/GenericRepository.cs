// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Model
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbContext context;

        public GenericRepository(DbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await this.context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await this.context.Set<T>().FindAsync(id);
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
