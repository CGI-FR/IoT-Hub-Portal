// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using System.Threading.Tasks;

    public interface IRoleRepository
    {
        Task<Role[]> GetAllAsync();
    }
}
