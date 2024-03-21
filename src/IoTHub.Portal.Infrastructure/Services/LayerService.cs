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

    public class LayerService : ILayerService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILayerRepository levelRepository;

        public LayerService(IMapper mapper,
            IUnitOfWork unitOfWork,
            ILayerRepository levelRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.levelRepository = levelRepository;
        }

        /// <summary>
        /// Create a level.
        /// </summary>
        /// <param name="level">Layer</param>
        /// <returns>Layer object.</returns>
        public async Task<LayerDto> CreateLayer(LayerDto level)
        {
            var levelEntity = this.mapper.Map<Layer>(level);

            await this.levelRepository.InsertAsync(levelEntity);
            await this.unitOfWork.SaveAsync();

            return level;
        }

        /// <summary>
        /// Update the level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdateLayer(LayerDto level)
        {
            var levelEntity = await this.levelRepository.GetByIdAsync(level.Id);

            if (levelEntity == null)
            {
                throw new ResourceNotFoundException($"The level with id {level.Id} doesn't exist");
            }

            _ = this.mapper.Map(level, levelEntity);

            this.levelRepository.Update(levelEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Delete level template
        /// </summary>
        /// <param name="levelId">The level indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteLayer(string levelId)
        {
            var levelEntity = await this.levelRepository.GetByIdAsync(levelId);
            if (levelEntity == null)
            {
                return;
            }

            this.levelRepository.Delete(levelId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get level.
        /// </summary>
        /// <param name="levelId">level id.</param>
        /// <returns>Layer object.</returns>
        public async Task<Layer> GetLayer(string levelId)
        {
            var levelEntity = await this.levelRepository.GetByIdAsync(levelId);

            if (levelEntity is null)
            {
                throw new ResourceNotFoundException($"The level with id {levelId} doesn't exist");
            }

            var level = this.mapper.Map<Layer>(levelEntity);

            return level;
        }

        /// <summary>
        /// Return the level list.
        /// </summary>
        /// <returns>IEnumerable LayerDto.</returns>
        public async Task<IEnumerable<LayerDto>> GetLayers()
        {
            var levelPredicate = PredicateBuilder.True<LayerDto>();

            var levels = await this.levelRepository.GetAllAsync();

            return levels
                .Select(model =>
                {
                    var levelListItem = this.mapper.Map<LayerDto>(model);
                    return levelListItem;
                })
                .ToList();
        }
    }
}
