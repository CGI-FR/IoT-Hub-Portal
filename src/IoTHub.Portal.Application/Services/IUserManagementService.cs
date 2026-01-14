// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    public interface IUserManagementService
    {
        Task<PaginatedResult<UserModel>> GetUserPage(
            string? searchName = null,
            string? searchEmail = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[]? orderBy = null
        );
        Task<UserDetailsModel> GetUserDetailsAsync(string userId);
        Task<UserDetailsModel> CreateUserAsync(UserDetailsModel userCreateModel);
        Task<UserDetailsModel?> UpdateUser(string id, UserDetailsModel user);
        Task<bool> DeleteUser(string userId);
        Task<UserDetailsModel> GetOrCreateUserByEmailAsync(string email, ClaimsPrincipal principal);
    }
}
