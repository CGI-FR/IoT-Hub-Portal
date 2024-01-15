// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IUnitOfWork unitOfWork;

        public RoleManagementService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            this.roleRepository = roleRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await this.roleRepository.GetAllAsync();
        }
        public async Task<Role> GetRoleDetailsAsync(string roleName)
        {
            return await this.roleRepository.GetByNameAsync(roleName, r => r.Actions);
        }

        public async Task<Role> CreateRole(Role roleEntity)
        {
            ArgumentNullException.ThrowIfNull(roleEntity, nameof(roleEntity));
            await roleRepository.InsertAsync(roleEntity);
            await unitOfWork.SaveAsync();
            return roleEntity;
        }

        public async Task<Role?> UpdateRole(Role role)
        {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            var roleEntity = await roleRepository.GetByIdAsync(role.Id);
            if (roleEntity == null) return null;

            // Mise à jour des propriétés
            roleEntity.Name = role.Name;
            roleEntity.Description = role.Description;
            roleEntity.Actions = role.Actions;

            roleRepository.Update(roleEntity);
            await unitOfWork.SaveAsync();
            return roleEntity;
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
