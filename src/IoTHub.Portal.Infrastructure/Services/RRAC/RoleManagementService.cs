// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public RoleManagementService(IRoleRepository roleRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.roleRepository = roleRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<RoleModel>> GetAllRolesAsync()
        {
            var roles = await roleRepository.GetAllAsync();
            var roleModel = roles.Select(role => mapper.Map<RoleModel>(role));
            return roleModel;
        }
        public async Task<RoleDetailsModel> GetRoleDetailsAsync(string roleName)
        {
            var role = await roleRepository.GetByNameAsync(roleName, r => r.Actions);
            if (role == null) return null;
            return mapper.Map<RoleDetailsModel>(role);
        }

        public async Task<RoleDetailsModel> CreateRole(RoleDetailsModel roleModel)
        {
            ArgumentNullException.ThrowIfNull(roleModel, nameof(roleModel));
            var roleEntity = mapper.Map<Role>(roleModel);

            foreach (var actionName in roleModel.Actions)
            {
                var actionEntity = new Action { Name = actionName };
                roleEntity.Actions.Add(actionEntity);
            }

            await roleRepository.InsertAsync(roleEntity);

            await unitOfWork.SaveAsync();

            return mapper.Map<RoleDetailsModel>(roleEntity);
        }

        public async Task<RoleDetailsModel> UpdateRole(string roleName, RoleDetailsModel roleDetails)
        {
            var actionsToRemove = new List<Action>();

            ArgumentNullException.ThrowIfNull(roleDetails, nameof(roleDetails));
            var roleEntity = await roleRepository.GetByNameAsync(roleName, r => r.Actions);
            if (roleEntity == null) return null;

            roleEntity.Name = roleDetails.Name;
            roleEntity.Description = roleDetails.Description;

            foreach (var actionName in roleDetails.Actions)
            {
                if (roleEntity.Actions.Any(a => a.Name == actionName)) continue;
                var actionEntity = new Action { Name = actionName };
                roleEntity.Actions.Add(actionEntity);
            }
            foreach (var action in roleEntity.Actions)
            {
                if (!roleDetails.Actions.Contains(action.Name))
                {
                    actionsToRemove.Add(action);
                }
            }
            foreach (var action in actionsToRemove)
            {
                _ = roleEntity.Actions.Remove(action);
            }

            roleRepository.Update(roleEntity);
            await unitOfWork.SaveAsync();

            return mapper.Map<RoleDetailsModel>(roleEntity);
        }

        public async Task<bool> DeleteRole(string roleName)
        {
            var actionsToRemove = new List<Action>();
            var role = await roleRepository.GetByNameAsync(roleName, r => r.Actions);
            if (role == null)
            {
                return false;
            }
            foreach (var action in role.Actions)
            {
                actionsToRemove.Add(action);
            }
            foreach (var action in actionsToRemove)
            {
                _ = role.Actions.Remove(action);
            }
            roleRepository.Delete(role.Id);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
