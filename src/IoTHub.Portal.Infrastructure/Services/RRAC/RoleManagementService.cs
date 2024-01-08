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

    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IMapper mapper;

        public RoleManagementService(IRoleRepository roleRepository, IMapper mapper)
        {
            this.roleRepository = roleRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<RoleModel>> GetAllRolesAsync()
        {
            var roles = await roleRepository.GetAllAsync();
            var roleModel = roles.Select(role => mapper.Map<RoleModel>(role));
            return roleModel;
        }
        public async Task<RoleDetailsModel> GetRoleDetailsAsync(string roleId)
        {
            var role = await roleRepository.GetByIdAsync(roleId);
            if (role == null) return null;
            return mapper.Map<RoleDetailsModel>(role);
        }

        // TODO : Other methods
    }
}
