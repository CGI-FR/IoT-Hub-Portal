// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Domain.Entities;

    public interface IRoleManagementService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleDetailsAsync(string roleName);
        Task<Role> CreateRole(Role roleEntity);
        Task<Role?> UpdateRole(Role roleEntity);
        Task<bool> DeleteRole(string roleName);
    }
}
