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
        private readonly ILayerRepository layerRepository;

        public LayerService(IMapper mapper,
            IUnitOfWork unitOfWork,
            ILayerRepository layerRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.layerRepository = layerRepository;
        }

        /// <summary>
        /// Create a layer.
        /// </summary>
        /// <param name="layer">Layer</param>
        /// <returns>Layer object.</returns>
        public async Task<LayerDto> CreateLayer(LayerDto layer)
        {
            var layerEntity = this.mapper.Map<Layer>(layer);

            await this.layerRepository.InsertAsync(layerEntity);
            await this.unitOfWork.SaveAsync();

            return layer;
        }

        /// <summary>
        /// Update the layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task UpdateLayer(LayerDto layer)
        {
            var layerEntity = await this.layerRepository.GetByIdAsync(layer.Id);

            if (layerEntity == null)
            {
                throw new ResourceNotFoundException($"The layer with id {layer.Id} doesn't exist");
            }

            _ = this.mapper.Map(layer, layerEntity);

            this.layerRepository.Update(layerEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Delete layer template
        /// </summary>
        /// <param name="layerId">The layer indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteLayer(string layerId)
        {
            var layerEntity = await this.layerRepository.GetByIdAsync(layerId);
            if (layerEntity == null)
            {
                throw new ResourceNotFoundException($"The layer with id {layerId} doesn't exist");
            }

            this.layerRepository.Delete(layerId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get layer.
        /// </summary>
        /// <param name="layerId">layer id.</param>
        /// <returns>Layer object.</returns>
        public async Task<Layer> GetLayer(string layerId)
        {
            var layerEntity = await this.layerRepository.GetByIdAsync(layerId);

            if (layerEntity is null)
            {
                throw new ResourceNotFoundException($"The layer with id {layerId} doesn't exist");
            }

            var layer = this.mapper.Map<Layer>(layerEntity);

            return layer;
        }

        /// <summary>
        /// Return the layer list.
        /// </summary>
        /// <returns>IEnumerable LayerDto.</returns>
        public async Task<IEnumerable<LayerDto>> GetLayers()
        {
            var layerPredicate = PredicateBuilder.True<LayerDto>();

            var layers = await this.layerRepository.GetAllAsync();

            return layers
                .Select(model =>
                {
                    var layerListItem = this.mapper.Map<LayerDto>(model);
                    return layerListItem;
                })
                .ToList();
        }
    }
}
