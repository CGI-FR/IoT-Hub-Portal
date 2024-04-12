// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IRoleManagementService
    {
        Task<PaginatedResult<RoleModel>> GetRolePage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null
        );
        Task<RoleDetailsModel> GetRoleDetailsAsync(string roleName);
        Task<RoleDetailsModel> CreateRole(RoleDetailsModel role);
        Task<RoleDetailsModel?> UpdateRole(string roleName, RoleDetailsModel role);
        Task<bool> DeleteRole(string roleName);
    }
}
