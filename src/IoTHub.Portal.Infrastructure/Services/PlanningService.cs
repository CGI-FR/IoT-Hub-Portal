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
        public async Task<PlanningDto> CreatePlanning(PlanningDto planning)
        {
            var planningEntity = this.mapper.Map<Planning>(planning);

            await this.planningRepository.InsertAsync(planningEntity);
            await this.unitOfWork.SaveAsync();

            return planning;
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
    }
}
