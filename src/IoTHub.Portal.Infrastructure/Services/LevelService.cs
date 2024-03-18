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

    public class LevelService : ILevelService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILevelRepository levelRepository;

        public LevelService(IMapper mapper,
            IUnitOfWork unitOfWork,
            ILevelRepository levelRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.levelRepository = levelRepository;
        }

        /// <summary>
        /// Create a level.
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Level object.</returns>
        public async Task<LevelDto> CreateLevel(LevelDto level)
        {
            var levelEntity = this.mapper.Map<Level>(level);

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
        public async Task UpdateLevel(LevelDto level)
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
        public async Task DeleteLevel(string levelId)
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
        /// <returns>Level object.</returns>
        public async Task<Level> GetLevel(string levelId)
        {
            var levelEntity = await this.levelRepository.GetByIdAsync(levelId);

            if (levelEntity is null)
            {
                throw new ResourceNotFoundException($"The level with id {levelId} doesn't exist");
            }

            var level = this.mapper.Map<Level>(levelEntity);

            return level;
        }

        /// <summary>
        /// Return the level list.
        /// </summary>
        /// <returns>IEnumerable LevelDto.</returns>
        public async Task<IEnumerable<LevelDto>> GetLevels()
        {
            var levelPredicate = PredicateBuilder.True<LevelDto>();

            var levels = await this.levelRepository.GetAllAsync();

            return levels
                .Select(model =>
                {
                    var levelListItem = this.mapper.Map<LevelDto>(model);
                    return levelListItem;
                })
                .ToList();
        }
    }
}
