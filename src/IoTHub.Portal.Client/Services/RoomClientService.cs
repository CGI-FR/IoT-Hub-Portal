// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public class RoomClientService : IRoomClientService
    {
        private readonly HttpClient http;
        private readonly string apiUrlBase = "api/building";

        public RoomClientService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> CreateRoom(RoomDto room)
        {
            var response = await this.http.PostAsJsonAsync(this.apiUrlBase, room);

            //Retrieve Room ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedRoom = Newtonsoft.Json.JsonConvert.DeserializeObject<RoomDto>(responseJson);

            return updatedRoom.Id.ToString();
        }

        public Task UpdateRoom(RoomDto room)
        {
            return this.http.PutAsJsonAsync(this.apiUrlBase, room);
        }

        public Task DeleteRoom(string roomId)
        {
            return this.http.DeleteAsync($"{this.apiUrlBase}/{roomId}");
        }

        public Task<RoomDto> GetRoom(string roomId)
        {
            return this.http.GetFromJsonAsync<RoomDto>($"{this.apiUrlBase}/{roomId}")!;
        }

        public async Task<List<RoomDto>> GetRooms()
        {
            return await this.http.GetFromJsonAsync<List<RoomDto>>(this.apiUrlBase) ?? new List<RoomDto>();
        }
    }
}
