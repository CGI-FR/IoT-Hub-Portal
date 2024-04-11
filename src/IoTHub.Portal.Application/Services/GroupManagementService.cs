// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*
namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain;

    public class GroupManagementService : IGroupManagementService
    {
        private readonly IGroupRepository groupRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public GroupManagementService(IGroupRepository groupRepository, IMapper mapper, IUnitOfWork unitOfWork, IRoleRepository roleRepository)
        {
            this.groupRepository = groupRepository;
            this.roleRepository = roleRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            return await this.groupRepository.GetAllAsync();
        }
        public async Task<Group> GetGroupDetailsAsync(string groupId)
        {
            return await this.groupRepository.GetByIdAsync(groupId);
        }

        public async Task<Group> CreateGroup(GroupDetailsModel groupModel)
        {
            ArgumentNullException.ThrowIfNull(groupModel, nameof(groupModel));
            var groupEntity = mapper.Map<Group>(groupModel);

            foreach (var accessControlModel in groupModel.AccessControls)
            {
                var role = await roleRepository.GetByNameAsync(accessControlModel.RoleName);
                if (role == null)
                {
                    continue;
                }

                var accessControlEntity = new AccessControl
                {
                    Scope = accessControlModel.Scope,
                    RoleName = role.Id,
                    GroupId = groupEntity.Id
                };
                groupEntity.AccessControls.Add(accessControlEntity);
            }

            foreach (var userModel in groupModel.Users)
            {
                var userMemberShip = new UserMemberShip
                {
                    UserId = userModel.Id,
                    GroupId = groupEntity.Id
                };
                groupEntity.Members.Add(userMemberShip);
            }

            await groupRepository.InsertAsync(groupEntity);
            await unitOfWork.SaveAsync();
            if (groupEntity == null)
            {
                return null;
            }

            return await this.GetGroupDetailsAsync(groupEntity.Id);
        }


        // TODO : Other methods
    }
}
 */