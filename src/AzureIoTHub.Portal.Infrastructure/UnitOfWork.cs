// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using AzureIoTHub.Portal.Domain;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;

    public class UnitOfWork<TContext> : IUnitOfWork, IDisposable
         where TContext : DbContext
    {
        private bool disposed;

        public UnitOfWork(TContext context)
        {
            this.Context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TContext Context { get; }

        public async Task SaveAsync()
        {
            _ = await this.Context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                this.Context.Dispose();
            }

            disposed = true;
        }
    }
}
