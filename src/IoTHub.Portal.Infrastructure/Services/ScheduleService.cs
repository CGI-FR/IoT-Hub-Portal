// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using InvalidPlanningScheduleException = Domain.Exceptions.InvalidPlanningScheduleException;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;

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
            var timeline = await this.GetPlanningScheduleDtos(schedule.PlanningId);
            timeline.Add(schedule);
            ValidateTimeline(timeline);

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

            if (schedule.PlanningId != scheduleEntity.PlanningId)
            {
                throw new InvalidPlanningScheduleException("A schedule cannot be moved to another planning.");
            }

            var timeline = await this.GetPlanningScheduleDtos(scheduleEntity.PlanningId);
            var scheduleIndex = timeline.FindIndex(existingSchedule => existingSchedule.Id == schedule.Id);
            if (scheduleIndex < 0)
            {
                throw new ResourceNotFoundException($"The schedule with id {schedule.Id} doesn't exist");
            }

            timeline[scheduleIndex] = schedule;
            ValidateTimeline(timeline);

            _ = this.mapper.Map(schedule, scheduleEntity);

            this.scheduleRepository.Update(scheduleEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Replace all slots for a planning after validating their complete timeline.
        /// </summary>
        /// <param name="planningId">The planning identifier.</param>
        /// <param name="schedules">The complete set of business-day slots.</param>
        public async Task ReplacePlanningSchedules(string planningId, IReadOnlyCollection<ScheduleDto> schedules)
        {
            var validationError = PlanningScheduleValidator.ValidateTimeline(schedules);
            if (validationError is not null)
            {
                throw new InvalidPlanningScheduleException(validationError);
            }

            var existingSchedules = (await this.scheduleRepository.GetAllAsync(schedule => schedule.PlanningId == planningId)).ToList();
            var existingById = existingSchedules.ToDictionary(schedule => schedule.Id);
            var submittedIds = new HashSet<string>();

            foreach (var schedule in schedules)
            {
                if (!submittedIds.Add(schedule.Id))
                {
                    throw new InvalidPlanningScheduleException($"The schedule id {schedule.Id} is duplicated.");
                }

                if (existingById.TryGetValue(schedule.Id, out var scheduleEntity))
                {
                    _ = this.mapper.Map(schedule, scheduleEntity);
                    scheduleEntity.PlanningId = planningId;
                    this.scheduleRepository.Update(scheduleEntity);
                    continue;
                }

                if (await this.scheduleRepository.GetByIdAsync(schedule.Id) is not null)
                {
                    throw new InvalidPlanningScheduleException($"The schedule id {schedule.Id} belongs to another planning.");
                }

                schedule.PlanningId = planningId;
                var newSchedule = this.mapper.Map<Schedule>(schedule);
                await this.scheduleRepository.InsertAsync(newSchedule);
            }

            foreach (var existingSchedule in existingSchedules.Where(schedule => !submittedIds.Contains(schedule.Id)))
            {
                this.scheduleRepository.Delete(existingSchedule.Id);
            }

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
                throw new ResourceNotFoundException($"The schedule with id {scheduleId} doesn't exist");
            }

            var timeline = await this.GetPlanningScheduleDtos(scheduleEntity.PlanningId);
            _ = timeline.RemoveAll(schedule => schedule.Id == scheduleId);
            ValidateTimeline(timeline);

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

        private async Task<List<ScheduleDto>> GetPlanningScheduleDtos(string planningId)
        {
            var schedules = await this.scheduleRepository.GetAllAsync(schedule => schedule.PlanningId == planningId);
            return schedules.Select(schedule => this.mapper.Map<ScheduleDto>(schedule)).ToList();
        }

        private static void ValidateTimeline(IReadOnlyCollection<ScheduleDto> schedules)
        {
            var validationError = PlanningScheduleValidator.ValidateTimeline(schedules);
            if (validationError is not null)
            {
                throw new InvalidPlanningScheduleException(validationError);
            }
        }
    }
}
