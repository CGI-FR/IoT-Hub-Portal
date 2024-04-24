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
        private readonly IUserRepository userRepository;
        public GroupService(IGroupRepository groupRepository, IMapper mapper, IUnitOfWork unitOfWork, IPrincipalRepository principalRepository, IUserRepository userRepository)
        {
            this.groupRepository = groupRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.principalRepository = principalRepository;
            this.userRepository = userRepository;
        }
        public async Task<GroupDetailsModel> GetGroupDetailsAsync(string groupId)
        {
            var groupEntity = await groupRepository.GetByIdAsync(groupId, g => g.Members);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException($"The group with the id {groupId} doesn't exist");
            }
            var groupModel = mapper.Map<GroupDetailsModel>(groupEntity);
            return groupModel;
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

            var createdGroupEntity = await this.groupRepository.GetByIdAsync(userEntity.Id);
            var mappedGroup = this.mapper.Map<GroupDetailsModel>(createdGroupEntity);
            return mappedGroup;
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

        public Task<GroupDetailsModel?> UpdateGroup(GroupDetailsModel group)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddUserToGroup(string groupId, string userId)
        {
            var userEntity = await this.userRepository.GetByIdAsync(userId);
            if (userEntity is null)
            {
                throw new ResourceNotFoundException("$The User with the id {userId} does'nt exist !");
            }
            var groupEntity = await this.groupRepository.GetByIdAsync(groupId, g => g.Members);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException("$The group with the id {groupId} does'nt exist !");
            }
            var existingMember = groupEntity.Members.FirstOrDefault(u => u.Id == userId);
            if (existingMember is null)
            {
                groupEntity.Members.Add(userEntity);
                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new ResourceAlreadyExistsException($"The user with the id {userId} is already a member of this group !");
            }
        }

        public async Task<bool> RemoveUserFromGroup(string groupId, string userId)
        {
            var groupEntity = await this.groupRepository.GetByIdAsync(groupId, g => g.Members);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException("$The group with the id {groupId} does'nt exist !");
            }
            var userEntity = groupEntity.Members.FirstOrDefault(userEntity => userEntity.Id == userId);
            if (userEntity is null)
            {
                throw new ResourceNotFoundException("$The User with the id {userId} is not a member of the group !");
            }
            _ = groupEntity.Members.Remove(userEntity);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
