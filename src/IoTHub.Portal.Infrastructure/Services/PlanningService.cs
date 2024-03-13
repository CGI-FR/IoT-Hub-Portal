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

    public class PlanningService : IPlanningService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPlanningRepository planningRepository;

        public PlanningService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IPlanningRepository planningRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.planningRepository = planningRepository;
        }

        /// <summary>
        /// Create a planning.
        /// </summary>
        /// <param name="planning">Planning</param>
        /// <returns>Planning object.</returns>
        public async Task<PlanningDto> CreatePlanning(PlanningDto planning)
        {
            var planningEntity = this.mapper.Map<Planning>(planning);

            await this.planningRepository.InsertAsync(planningEntity);
            await this.unitOfWork.SaveAsync();

            return planning;
        }

        /// <summary>
        /// Update the planning.
        /// </summary>
        /// <param name="planning">The planning.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdatePlanning(PlanningDto planning)
        {
            var planningEntity = await this.planningRepository.GetByIdAsync(planning.Id);

            if (planningEntity == null)
            {
                throw new ResourceNotFoundException($"The planning with id {planning.Id} doesn't exist");
            }

            _ = this.mapper.Map(planning, planningEntity);

            this.planningRepository.Update(planningEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Delete planning template
        /// </summary>
        /// <param name="planningId">The planning indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeletePlanning(string planningId)
        {
            var planningEntity = await this.planningRepository.GetByIdAsync(planningId);
            if (planningEntity == null)
            {
                return;
            }

            this.planningRepository.Delete(planningId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get planning.
        /// </summary>
        /// <param name="planningId">planning id.</param>
        /// <returns>Planning object.</returns>
        public async Task<Planning> GetPlanning(string planningId)
        {
            var planningEntity = await this.planningRepository.GetByIdAsync(planningId);

            if (planningEntity is null)
            {
                throw new ResourceNotFoundException($"The planning with id {planningId} doesn't exist");
            }

            var planning = this.mapper.Map<Planning>(planningEntity);

            return planning;
        }

        /// <summary>
        /// Return the planning list.
        /// </summary>
        /// <returns>IEnumerable PlanningDto.</returns>
        public async Task<IEnumerable<PlanningDto>> GetPlannings()
        {
            var planningPredicate = PredicateBuilder.True<PlanningDto>();

            var plannings = await this.planningRepository.GetAllAsync();

            return plannings
                .Select(model =>
                {
                    var planningListItem = this.mapper.Map<PlanningDto>(model);
                    return planningListItem;
                })
                .ToList();
        }
    }
}
