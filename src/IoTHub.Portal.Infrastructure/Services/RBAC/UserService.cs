// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class UserService : IUserManagementService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAccessControlRepository acRepository;
        private readonly IRoleRepository roleRepository;

        public UserService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IAccessControlRepository acRepository, IRoleRepository roleRepository)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.acRepository = acRepository;
            this.roleRepository = roleRepository;
        }
        public async Task<UserDetailsModel> GetUserDetailsAsync(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId, u => u.AccessControls.ThenInclude(ac => ac.Role));
            if (user == null) return null;
            return mapper.Map<UserDetailsModel>(user);
        }

        public async Task<UserDetailsModel> CreateUserAsync(UserDetailsModel userCreateModel)
        {
            var userEntity = this.mapper.Map<User>(userCreateModel);
            await userRepository.InsertAsync(userEntity);
            var acEntities = new Collection<AccessControl>();
            foreach (var ac in userCreateModel.AccessControls)
            {
                var role = await this.roleRepository.GetByIdAsync(ac.Role.Id);
                if (role is null)
                {
                    throw new ResourceNotFoundException("$The Role {ac.Role.Name} does'nt exist !");
                }
                var acEntity = new AccessControl
                {
                    Scope = ac.Scope,
                    RoleId = ac.Role.Id,
                };
                acEntities.Add(acEntity);
            }
            foreach (var ac in acEntities)
            {
                userEntity.AccessControls.Add(ac);
            }

            await unitOfWork.SaveAsync();

            var createdEntity = await this.userRepository.GetByIdAsync(userEntity.Id);
            var createdModel = this.mapper.Map<UserDetailsModel>(createdEntity);
            return createdModel;
        }

        public async Task<PaginatedResult<UserModel>> GetUserPage(
            string? searchName,
            string? searchEmail,
            int pageSize,
            int pageNumber,
            string[] orderBy
            )
        {
            var userFilter = new UserFilter
            {
                SearchName = searchName,
                SearchEmail = searchEmail,
                PageSize = pageSize,
                PageNumber = pageNumber,
                OrderBy = orderBy
            };

            var userPredicate = PredicateBuilder.True<User>();
            if (!string.IsNullOrWhiteSpace(userFilter.SearchName))
            {
                userPredicate = userPredicate.And(user => user.Name.ToLower().Contains(userFilter.SearchName.ToLower())
                || user.FamilyName.ToLower().Contains(userFilter.SearchName.ToLower()) ||
                user.GivenName.ToLower().Contains(userFilter.SearchName.ToLower())
                );
            }
            var paginatedUser = await this.userRepository.GetPaginatedListAsync(pageNumber, pageSize, orderBy, userPredicate);
            var paginatedUserDto = new PaginatedResult<UserModel>
            {
                Data = paginatedUser.Data.Select(x => this.mapper.Map<UserModel>(x)).ToList(),
                TotalCount = paginatedUser.TotalCount,
                CurrentPage = paginatedUser.CurrentPage,
                PageSize = pageSize
            };

            return new PaginatedResult<UserModel>(paginatedUserDto.Data, paginatedUserDto.TotalCount);
        }

        public async Task<UserDetailsModel?> UpdateUser(UserDetailsModel user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteUser(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new ResourceNotFoundException("$The User with the id {userId} that you want to delete does'nt exist !");
            }
            userRepository.Delete(userId);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
