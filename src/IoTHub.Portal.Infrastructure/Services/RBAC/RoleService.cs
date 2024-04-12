// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services.RBAC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Action = Domain.Entities.Action;

    internal class RoleService : IRoleManagementService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.roleRepository = roleRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
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

        public async Task<PaginatedResult<RoleModel>> GetRolePage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null)
        {
            var roleFilter = new  RoleFilter
            {
                Keyword = searchKeyword,
                PageSize = pageSize,
                PageNumber = pageNumber,
                OrderBy = orderBy
            };

            var rolePredicate = PredicateBuilder.True<Role>();
            if (!string.IsNullOrWhiteSpace(roleFilter.Keyword))
            {
                rolePredicate = rolePredicate.And(role => role.Name.ToLower().Contains(roleFilter.Keyword.ToLower()) || role.Description.ToLower().Contains(roleFilter.Keyword.ToLower()));
            }

            var paginatedRole = await this.roleRepository.GetPaginatedListAsync(pageNumber,
                pageSize,
                orderBy,
                rolePredicate,
                includes: new Expression<Func<Role, object>>[] { role => role.Actions});

            var paginatedRoleDto = new PaginatedResult<RoleModel>
            {
                Data = paginatedRole.Data.Select(x => this.mapper.Map<RoleModel>(x)).ToList(),
                TotalCount = paginatedRole.TotalCount,
                CurrentPage = paginatedRole.CurrentPage,
                PageSize = pageSize
            };

            return new PaginatedResult<RoleModel>(paginatedRoleDto.Data, paginatedRoleDto.TotalCount);

        }
    }
}
