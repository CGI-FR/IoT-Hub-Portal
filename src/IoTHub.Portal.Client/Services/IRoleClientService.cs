// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Security;

    public interface IRoleClientService
    {
        Task<PaginationResult<RoleModel>> GetRoles(string continuationUri);

        Task DeleteRole(string roleId);

        Task<RoleDetailsModel> GetRole(string groupId);

        Task CreateRole(RoleDetailsModel role);

        Task UpdateRole(RoleDetailsModel role);

        Task<PortalPermissions[]> GetPermissions();
    }
}
