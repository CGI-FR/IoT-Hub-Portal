// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();

        Task<T> GetByIdAsync(object id);

        Task InsertAsync(T obj);

        void Update(T obj);

        void Delete(object id);
    }
}
