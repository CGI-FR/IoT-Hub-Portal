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
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using System.Linq.Expressions;

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

        public async Task<RoomDto> CreateRoom(RoomDto room)
        {
            var roomEntity = this.mapper.Map<Room>(room);

            await this.roomRepository.InsertAsync(roomEntity);
            await this.unitOfWork.SaveAsync();

            return room;
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
    }
}
