// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Shared.Models.v1._0;

    public interface IRoleManagementService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(string roleId);
    }
}
