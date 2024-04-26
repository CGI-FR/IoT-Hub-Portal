// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Repositories
{
    using System.Linq.Expressions;
    using IoTHub.Portal.Domain.Entities;

    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByNameAsync(string userName, params Expression<Func<User, object>>[] includeProperties);
    }

}
