namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using System.Collections.Generic;
    using System.Linq;
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

        public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await groupRepository.GetAllAsync();

            var groupDtos = groups.Select(group => mapper.Map<GroupDto>(group));

            return groupDtos;
        }
        public async Task<GroupDto> GetGroupByIdAsync(string groupId)
        {
            var group = await groupRepository.GetByIdAsync(groupId);
            return group != null ? mapper.Map<GroupDto>(group) : null;
        }
    }
}
