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
        private readonly IAccessControlRepository accessControlRepository;
        public GroupService(IGroupRepository groupRepository, IMapper mapper, IUnitOfWork unitOfWork,
            IPrincipalRepository principalRepository, IUserRepository userRepository, IAccessControlRepository accessControlRepository)
        {
            this.groupRepository = groupRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.principalRepository = principalRepository;
            this.userRepository = userRepository;
            this.accessControlRepository = accessControlRepository;
        }
        public async Task<GroupDetailsModel> GetGroupDetailsAsync(string id)
        {
            var groupEntity = await groupRepository.GetByIdAsync(id, g => g.Members);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException($"The group with the id {id} doesn't exist");
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

        public async Task<GroupDetailsModel> CreateGroupAsync(GroupDetailsModel group)
        {
            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            var existingName = await this.groupRepository.GetByNameAsync(group.Name);
            if (existingName is not null)
            {
                throw new ResourceAlreadyExistsException($"The Group with the name {group.Name} already exist !");
            }
            var groupEntity = this.mapper.Map<Group>(group);
            await groupRepository.InsertAsync(groupEntity);
            await unitOfWork.SaveAsync();

            var createdGroup = await this.groupRepository.GetByIdAsync(groupEntity.Id);
            return this.mapper.Map<GroupDetailsModel>(createdGroup);
        }

        public async Task<bool> DeleteGroup(string id)
        {
            var group = await groupRepository.GetByIdAsync(id);
            if (group is null)
            {
                throw new ResourceNotFoundException($"The Group with the id {id} that you want to delete doesn't exist !");
            }
            principalRepository.Delete(group.PrincipalId);
            groupRepository.Delete(id);
            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<GroupDetailsModel?> UpdateGroup(string id, GroupDetailsModel group)
        {
            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            var groupEntity = await this.groupRepository.GetByIdAsync(id);
            if (groupEntity is null) throw new ResourceNotFoundException($"The group with id {id} does'nt exist");
            var existingName = await this.groupRepository.GetByNameAsync(group.Name);
            if (existingName is not null)
            {
                throw new ResourceAlreadyExistsException($"The Group tis the name {group.Name} already exist !");
            }
            groupEntity.Name = group.Name;
            groupEntity.Color = group.Color;
            groupEntity.Description = group.Description;
            this.groupRepository.Update(groupEntity);
            await this.unitOfWork.SaveAsync();
            var updatedGroup = await this.groupRepository.GetByIdAsync(id);
            return this.mapper.Map<GroupDetailsModel>(updatedGroup);
        }

        public async Task<bool> AddUserToGroup(string groupId, string userId)
        {
            var userEntity = await this.userRepository.GetByIdAsync(userId);
            if (userEntity is null)
            {
                throw new ResourceNotFoundException($"The User with the id {userId} does'nt exist !");
            }
            var groupEntity = await this.groupRepository.GetByIdAsync(groupId, g => g.Members);
            if (groupEntity is null)
            {
                throw new ResourceNotFoundException($"The group with the id {groupId} does'nt exist !");
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
                throw new ResourceNotFoundException($"The group with the id {groupId} does'nt exist !");
            }
            var userEntity = groupEntity.Members.FirstOrDefault(userEntity => userEntity.Id == userId);
            if (userEntity is null)
            {
                throw new ResourceNotFoundException($"The User with the id {userId} is not a member of the group !");
            }
            _ = groupEntity.Members.Remove(userEntity);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
