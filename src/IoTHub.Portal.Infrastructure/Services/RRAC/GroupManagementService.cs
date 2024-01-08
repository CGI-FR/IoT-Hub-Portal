// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class GroupManagementService : IGroupManagementService
    {
        private readonly IGroupRepository groupRepository;
        private readonly IMapper mapper;

        public GroupManagementService(IGroupRepository groupRepository, IMapper mapper)
        {
            this.groupRepository = groupRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<GroupModel>> GetAllGroupsAsync()
        {
            var groups = await groupRepository.GetAllAsync();
            var groupModel = groups.Select(group => mapper.Map<GroupModel>(group));
            return groupModel;
        }
        public async Task<GroupDetailsModel> GetGroupDetailsAsync(string groupId)
        {
            var group = await groupRepository.GetByIdAsync(groupId);
            if (group == null) return null;
            return mapper.Map<GroupDetailsModel>(group);
        }

        // TODO : Other methods
    }
}
