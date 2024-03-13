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

        /// <summary>
        /// Create a schedule.
        /// </summary>
        /// <param name="schedule">Schedule</param>
        /// <returns>Schedule object.</returns>
        public async Task<ScheduleDto> CreateSchedule(ScheduleDto schedule)
        {
            var scheduleEntity = this.mapper.Map<Schedule>(schedule);

            await this.scheduleRepository.InsertAsync(scheduleEntity);
            await this.unitOfWork.SaveAsync();

            return schedule;
        }

        /// <summary>
        /// Update the schedule.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdateSchedule(ScheduleDto schedule)
        {
            var scheduleEntity = await this.scheduleRepository.GetByIdAsync(schedule.Id);

            if (scheduleEntity == null)
            {
                throw new ResourceNotFoundException($"The schedule with id {schedule.Id} doesn't exist");
            }

            _ = this.mapper.Map(schedule, scheduleEntity);

            this.scheduleRepository.Update(scheduleEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Delete schedule template
        /// </summary>
        /// <param name="scheduleId">The schedule indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteSchedule(string scheduleId)
        {
            var scheduleEntity = await this.scheduleRepository.GetByIdAsync(scheduleId);
            if (scheduleEntity == null)
            {
                return;
            }

            this.scheduleRepository.Delete(scheduleId);

            await this.unitOfWork.SaveAsync();
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

        /// <summary>
        /// Return the schedule list.
        /// </summary>
        /// <returns>IEnumerable ScheduleDto.</returns>
        public async Task<IEnumerable<ScheduleDto>> GetSchedules()
        {
            var schedulePredicate = PredicateBuilder.True<ScheduleDto>();

            var schedules = await this.scheduleRepository.GetAllAsync();

            return schedules
                .Select(model =>
                {
                    var scheduleListItem = this.mapper.Map<ScheduleDto>(model);
                    return scheduleListItem;
                })
                .ToList();
        }
    }
}
