// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using AutoMapper;
    using IoTHub.Portal.Crosscutting;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Threading.Tasks;

    public class GroupService : IGroupManagementService
    {
        private readonly IGroupRepository groupRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPrincipalRepository principalRepository;
        public GroupService(IGroupRepository groupRepository, IMapper mapper, IUnitOfWork unitOfWork, IPrincipalRepository principalRepository)
        {
            this.groupRepository = groupRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.principalRepository = principalRepository;
        }
        public async Task<GroupDetailsModel> GetGroupDetailsAsync(string groupId)
        {
            var groupEntity = await groupRepository.GetByIdAsync(groupId);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException($"The group with the id {groupId} doesn't exist");
            }
            return mapper.Map<GroupDetailsModel>(groupEntity);
        }

        async Task<PaginatedResult<GroupModel>> IGroupManagementService.GetGroupPage(
            string? searchKeyword,
            int pageSize,
            int pageNumber,
            string[] orderBy)
        {
            var groupFilter = new GroupFilter
            {
                Keyword = searchKeyword,
                PageSize = pageSize,
                PageNumber = pageNumber,
                OrderBy = orderBy
            };

            var groupPredicate = PredicateBuilder.True<Group>();
            if (!string.IsNullOrWhiteSpace(groupFilter.Keyword))
            {
                groupPredicate = groupPredicate.And(grp => grp.Name.ToLower().Contains(groupFilter.Keyword.ToLower())
                || grp.Description.ToLower().Contains(groupFilter.Keyword.ToLower())
                );
            }
            var paginatedGroup = await this.groupRepository.GetPaginatedListAsync(pageNumber, pageSize, orderBy, groupPredicate);
            var paginatedGroupDto = new PaginatedResult<GroupModel>
            {
                Data = paginatedGroup.Data.Select(x => this.mapper.Map<GroupModel>(x)).ToList(),
                TotalCount = paginatedGroup.TotalCount,
                CurrentPage = paginatedGroup.CurrentPage,
                PageSize = pageSize,
            };
            return new PaginatedResult<GroupModel>(paginatedGroupDto.Data, paginatedGroupDto.TotalCount);
        }

        public async Task<GroupDetailsModel> CreateGroupAsync(GroupDetailsModel groupCreateModel)
        {
            var userEntity = this.mapper.Map<Group>(groupCreateModel);
            await groupRepository.InsertAsync(userEntity);
            await unitOfWork.SaveAsync();
            return groupCreateModel;
        }

        public async Task<bool> DeleteGroup(string groupId)
        {
            var group = await groupRepository.GetByIdAsync(groupId);
            if (group is null)
            {
                throw new ResourceNotFoundException("$The User with the id {userId} that you want to delete does'nt exist !");
            }
            principalRepository.Delete(group.PrincipalId);
            groupRepository.Delete(groupId);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
