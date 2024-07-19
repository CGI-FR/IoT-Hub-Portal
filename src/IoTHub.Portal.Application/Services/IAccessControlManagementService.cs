// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Shared.Models.v10;

    public interface IAccessControlManagementService
    {
        Task<PaginatedResult<AccessControlModel>> GetAccessControlPage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null,
            string? principalId = null
            );
        Task<AccessControlModel> GetAccessControlAsync(string Id);

        Task<AccessControlModel> CreateAccessControl(AccessControlModel role);
        Task<AccessControlModel?> UpdateAccessControl(string id, AccessControlModel accessControl);
        Task<bool> DeleteAccessControl(string id);
    }
}
