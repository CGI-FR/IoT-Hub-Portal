// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Repositories
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;

    public interface IMenuEntryRepository : IRepository<MenuEntry>
    {
        Task<MenuEntry?> GetByNameAsync(string name);
    }
}
