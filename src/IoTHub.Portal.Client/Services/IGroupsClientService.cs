// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IGroupsClientService
    {
        Task<PaginationResult<GroupModel>> GetGroups(string continuationUri);

        Task DeleteGroup(string grpId);

        Task<GroupDetailsModel> GetGroup(string groupId);

        Task CreateGroup(GroupDetailsModel group);

        Task UpdateGroup(string groupId, GroupDetailsModel group);

    }
}
