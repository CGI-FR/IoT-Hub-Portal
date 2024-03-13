// Copyright(c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using Domain;
    using Domain.Repositories;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Repositories;

    public class RoomService : IRoomService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IRoomRepository roomRepository;

        public RoomService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IRoomRepository roomRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.roomRepository = roomRepository;
        }

        /// <summary>
        /// Create a room.
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns>Room object.</returns>
        public async Task<RoomDto> CreateRoom(RoomDto room)
        {
            var roomEntity = this.mapper.Map<Room>(room);

            await this.roomRepository.InsertAsync(roomEntity);
            await this.unitOfWork.SaveAsync();

            return room;
        }

        /// <summary>
        /// Update the room.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdateRoom(RoomDto room)
        {
            var roomEntity = await this.roomRepository.GetByIdAsync(room.Id);

            if (roomEntity == null)
            {
                throw new ResourceNotFoundException($"The room with id {room.Id} doesn't exist");
            }

            _ = this.mapper.Map(room, roomEntity);

            this.roomRepository.Update(roomEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Delete room template
        /// </summary>
        /// <param name="roomId">The room indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteRoom(string roomId)
        {
            var roomEntity = await this.roomRepository.GetByIdAsync(roomId);
            if (roomEntity == null)
            {
                return;
            }

            this.roomRepository.Delete(roomId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get room.
        /// </summary>
        /// <param name="roomId">room id.</param>
        /// <returns>Room object.</returns>
        public async Task<Room> GetRoom(string roomId)
        {
            var roomEntity = await this.roomRepository.GetByIdAsync(roomId);

            if (roomEntity is null)
            {
                throw new ResourceNotFoundException($"The room with id {roomId} doesn't exist");
            }

            var room = this.mapper.Map<Room>(roomEntity);

            return room;
        }

        /// <summary>
        /// Return the room list.
        /// </summary>
        /// <returns>IEnumerable RoomDto.</returns>
        public async Task<IEnumerable<RoomDto>> GetRooms()
        {
            var roomPredicate = PredicateBuilder.True<RoomDto>();

            var rooms = await this.roomRepository.GetAllAsync();

            return rooms
                .Select(model =>
                {
                    var roomListItem = this.mapper.Map<RoomDto>(model);
                    return roomListItem;
                })
                .ToList();
        }
    }
}
