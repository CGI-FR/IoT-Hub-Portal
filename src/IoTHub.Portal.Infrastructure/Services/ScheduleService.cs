// Copyright (c) CGI France. All rights reserved.
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

    public class ScheduleService : IScheduleService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IScheduleRepository scheduleRepository;

        public ScheduleService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IScheduleRepository scheduleRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.scheduleRepository = scheduleRepository;
        }
        public async Task<ScheduleDto> CreateSchedule(ScheduleDto schedule)
        {
            var scheduleEntity = this.mapper.Map<Schedule>(schedule);

            await this.scheduleRepository.InsertAsync(scheduleEntity);
            await this.unitOfWork.SaveAsync();

            return schedule;
        }

        /// <summary>
        /// Get schedule.
        /// </summary>
        /// <param name="scheduleId">schedule id.</param>
        /// <returns>Schedule object.</returns>
        public async Task<Schedule> GetSchedule(string scheduleId)
        {
            var scheduleEntity = await this.scheduleRepository.GetByIdAsync(scheduleId);

            if (scheduleEntity is null)
            {
                throw new ResourceNotFoundException($"The schedule with id {scheduleId} doesn't exist");
            }

            var schedule = this.mapper.Map<Schedule>(scheduleEntity);

            return schedule;
        }
    }
}
