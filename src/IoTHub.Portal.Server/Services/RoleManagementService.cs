namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using System.Collections.Generic;
    using System.Linq;
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

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await roleRepository.GetAllAsync();

            var roleDtos = roles.Select(role => mapper.Map<RoleDto>(role));

            return roleDtos;
        }
        public async Task<RoleDto> GetRoleByIdAsync(string roleId)
        {
            var role = await roleRepository.GetByIdAsync(roleId);
            return role != null ? mapper.Map<RoleDto>(role) : null;
        }
    }
}
