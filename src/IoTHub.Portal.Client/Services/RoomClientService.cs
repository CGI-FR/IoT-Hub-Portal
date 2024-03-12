// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.WebUtilities;

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
            var response = await this.http.PostAsJsonAsync("api/building", room);

            //Retrieve Room ID
            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedRoom = Newtonsoft.Json.JsonConvert.DeserializeObject<RoomDto>(responseJson);

            return updatedRoom.Id.ToString();
        }


        public Task<RoomDto> GetRoom(string roomId)
        {
            return this.http.GetFromJsonAsync<RoomDto>($"api/building/{roomId}")!;
        }
    }
}
