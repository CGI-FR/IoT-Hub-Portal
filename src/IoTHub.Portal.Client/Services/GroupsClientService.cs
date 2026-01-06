// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;


    public class GroupsClientService : IGroupsClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/groups";
        public GroupsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task<PaginationResult<GroupModel>> GetGroups(string continuationUri)
        {
            return this.http.GetFromJsonAsync<PaginationResult<GroupModel>>(continuationUri)!;
        }

        public Task DeleteGroup(string deviceId)
        {
            return this.http.DeleteAsync($"api/groups/{deviceId}");
        }

        public async Task<GroupDetailsModel> GetGroup(string groupId)
        {
            return await this.http.GetFromJsonAsync<GroupDetailsModel>($"{this.apiUrlBase}/{groupId}") ?? new GroupDetailsModel();
        }

        public Task CreateGroup(GroupDetailsModel group)
        {
            return this.http.PostAsJsonAsync(this.apiUrlBase, group);
        }

        public Task UpdateGroup(string groupId, GroupDetailsModel group)
        {
            return this.http.PutAsJsonAsync($"{this.apiUrlBase}/{groupId}", group);
        }
    }
}
