// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository roleRepository;

        public RoleManagementService(IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await roleRepository.GetAllAsync();
            return (IEnumerable<RoleDto>)roles;
        }

        Task<IEnumerable<RoleDto>> IRoleManagementService.GetAllRolesAsync()
        {
            throw new System.NotImplementedException();
        }

        // TODO : Other methods
    }
}
