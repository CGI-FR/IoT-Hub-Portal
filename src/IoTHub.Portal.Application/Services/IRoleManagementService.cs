// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Shared.Models.v10;

    public interface IRoleManagementService
    {
        Task<IEnumerable<RoleModel>> GetAllRolesAsync();
        Task<RoleDetailsModel> GetRoleDetailsAsync(string roleName);
        Task<RoleDetailsModel> CreateRole(RoleDetailsModel roleDetails);
        Task<RoleDetailsModel> UpdateRole(string roleName, RoleDetailsModel roleDetails);
        Task<bool> DeleteRole(string roleName);
    }
}
