// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using IoTHub.Portal.Shared.Models.v10;
    using System.Threading.Tasks;

    public interface IUserClientService
    {
        Task<PaginationResult<UserModel>> GetUsers(string continuationUri);
        Task<UserDetailsModel> GetUser(string id);
        Task CreateUser(UserDetailsModel user);
        Task UpdateUser(UserDetailsModel user);
        Task DeleteUser(string id);
    }
}
