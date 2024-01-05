namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UserManagementService(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await userRepository.GetAllAsync();

            var userDtos = users.Select(user => mapper.Map<UserDto>(user));

            return userDtos;
        }
        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            return user != null ? mapper.Map<UserDto>(user) : null;
        }
    }
}
