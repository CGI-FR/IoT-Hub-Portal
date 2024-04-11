// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IRoleManagementService
    {
        Task<PaginationResult<RoleModel>> GetRolePage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string? orderBy = null
        );
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleDetailsAsync(string roleName);
        Task<Role> CreateRole(Role roleEntity);
        Task<Role?> UpdateRole(Role roleEntity);
        Task<bool> DeleteRole(string roleName);
    }
}
