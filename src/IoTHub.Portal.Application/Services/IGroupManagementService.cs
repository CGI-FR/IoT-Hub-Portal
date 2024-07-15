// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using IoTHub.Portal.Shared.Models.v10;

    public interface IGroupManagementService
    {
        Task<PaginatedResult<GroupModel>> GetGroupPage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null
        );
        Task<GroupDetailsModel> GetGroupDetailsAsync(string groupId);
        Task<GroupDetailsModel> CreateGroupAsync(GroupDetailsModel groupCreateModel);
        Task<bool> DeleteGroup(string userId);
        Task<GroupDetailsModel?> UpdateGroup(string id, GroupDetailsModel group);
        Task<bool> AddUserToGroup(string groupId, string userId);
        Task<bool> RemoveUserFromGroup(string groupId, string userId);
    }
}
