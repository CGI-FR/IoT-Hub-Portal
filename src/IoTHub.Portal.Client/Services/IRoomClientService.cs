// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IRoomClientService
    {
        Task<string> CreateRoom(RoomDto room);
        Task UpdateRoom(RoomDto room);
        Task DeleteRoom(string modelId);
        Task<RoomDto> GetRoom(string roomId);
        Task<List<RoomDto>> GetRooms();
    }
}
