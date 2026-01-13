// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using AutoMapper;
    using IoTHub.Portal.Crosscutting;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class UserService : IUserManagementService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPrincipalRepository principalRepository;
        private readonly IAccessControlRepository accessControlRepository;
        private readonly IRoleRepository roleRepository;


        public UserService(IMapper mapper, IUnitOfWork unitOfWork, IUserRepository userRepository, IPrincipalRepository principalRepository, IAccessControlRepository accessControlRepository, IRoleRepository roleRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.userRepository = userRepository;
            this.principalRepository = principalRepository;
            this.accessControlRepository = accessControlRepository;
            this.roleRepository = roleRepository;

        }
        public async Task<UserDetailsModel> GetUserDetailsAsync(string id)
        {
            var user = await userRepository.GetByIdAsync(id);// u => u.Groups);
            if (user == null) throw new ResourceNotFoundException($"The user with the id {id} doesn't exist");
            return mapper.Map<UserDetailsModel>(user);
        }

        public async Task<UserDetailsModel> CreateUserAsync(UserDetailsModel user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var existingName = await this.userRepository.GetByNameAsync(user.Name);
            if (existingName is not null)
            {
                throw new ResourceAlreadyExistsException($"The User tis the name {user.Name} already exist !");
            }
            var userEntity = this.mapper.Map<User>(user);
            await userRepository.InsertAsync(userEntity);

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

        public async Task<UserDetailsModel?> UpdateUser(string id, UserDetailsModel user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var userEntity = await this.userRepository.GetByIdAsync(id);
            if (userEntity == null) throw new ResourceNotFoundException($"The User with the id {id} does'nt exist !");
            var existingName = await this.userRepository.GetByNameAsync(user.Name);
            if (existingName is not null)
            {
                throw new ResourceAlreadyExistsException($"The User with the name {user.Name} already exist !");
            }

            userEntity.Email = user.Email;
            userEntity.GivenName = user.GivenName;
            userEntity.Name = user.Name;
            userEntity.FamilyName = user.FamilyName;
            userEntity.Avatar = user.Avatar;

            this.userRepository.Update(userEntity);
            await this.unitOfWork.SaveAsync();
            var createdUser = await this.userRepository.GetByIdAsync(id);
            var toModel = this.mapper.Map<UserDetailsModel>(createdUser);
            return toModel;

        }

        public async Task<bool> DeleteUser(string id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user is null)
            {
                throw new ResourceNotFoundException($"The User with the id {id} that you want to delete doesn't exist !");
            }

            principalRepository.Delete(user.PrincipalId);
            userRepository.Delete(id);
            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<UserDetailsModel> GetOrCreateUserByEmailAsync(string email, ClaimsPrincipal principal)
        {
            // Retrieve the user by email (case insensitive)
            var users = await this.userRepository.GetAllAsync(u => u.Email.ToLower() == email.ToLower());
            var userEntity = users.FirstOrDefault();

            if (userEntity != null)
            {
                return mapper.Map<UserDetailsModel>(userEntity);
            }

            // Check if it's the first user in the system, add to administrators role if so
            var isAny = (await this.userRepository.CountAsync()) > 0;

            // Extract user information from the ClaimsPrincipal
            var fullName = principal.FindFirst("name")?.Value ?? "";
            var preferredUsername = principal.FindFirst("preferred_username")?.Value ?? email;
            var familyName = principal.FindFirst("family_name")?.Value ?? "";

            // We ignore the "given_name" from the token since you want to use preferred_username
            // Create a new user
            var newUser = new User
            {
                Email = email,
                Name = fullName,
                GivenName = preferredUsername,
                FamilyName = familyName,
                PrincipalId = Guid.NewGuid().ToString(),
                //Principal = new Principal()
            };

            await this.userRepository.InsertAsync(newUser);
            await this.unitOfWork.SaveAsync();

            if (!isAny)
            {
                // If this is the first user, assign them to the Administrators role
                var adminRole = await this.roleRepository.GetByNameAsync("Administrators");

                if (adminRole == null)
                {
                    throw new ResourceNotFoundException(
                        "The 'Administrators' role does not exist. Cannot assign the first user to this role.");
                }

                var newAccessControl = new AccessControl
                {
                    Id = Guid.NewGuid().ToString(),
                    //Principal = newUser.Principal,
                    PrincipalId = newUser.PrincipalId,
                    RoleId = adminRole.Id,
                    Role = adminRole,
                    Scope = ""
                };

                await this.accessControlRepository.InsertAsync(newAccessControl);
                await this.unitOfWork.SaveAsync();
            }

            return mapper.Map<UserDetailsModel>(newUser);
        }
    }
}
