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

    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UserManagementService(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            var users = await userRepository.GetAllAsync();
            var userModel = users.Select(user => mapper.Map<UserModel>(user));
            return userModel;
        }
        public async Task<UserDetailsModel> GetUserDetailsAsync(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) return null;
            return mapper.Map<UserDetailsModel>(user);
        }

        // TODO : Other methods
    }
}
