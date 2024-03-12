// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;

    public interface IRoomService
    {
        Task<RoomDto> CreateRoom(RoomDto room);
        Task<PaginatedResult<RoomDto>> GetRooms(RoomFilter roomFilter);
        Task<Room> GetRoom(string roomId);
    }
}
