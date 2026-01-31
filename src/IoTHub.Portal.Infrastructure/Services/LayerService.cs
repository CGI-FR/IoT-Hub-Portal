// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using DeviceEntity = Domain.Entities.Device;
    using IncompatibleDeviceModelException = Domain.Exceptions.IncompatibleDeviceModelException;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;

    public class LayerService : ILayerService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILayerRepository layerRepository;
        private readonly IPlanningRepository planningRepository;
        private readonly IDeviceRepository deviceRepository;

        public LayerService(IMapper mapper,
            IUnitOfWork unitOfWork,
            ILayerRepository layerRepository,
            IPlanningRepository planningRepository,
            IDeviceRepository deviceRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.layerRepository = layerRepository;
            this.planningRepository = planningRepository;
            this.deviceRepository = deviceRepository;
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

            // Validate that if the layer is being linked to a planning with a device model,
            // all devices in the layer (and child layers) must have the same device model
            if (!string.IsNullOrEmpty(layer.Planning) && layer.Planning != "None")
            {
                var planning = await this.planningRepository.GetByIdAsync(layer.Planning);

                if (planning == null)
                {
                    throw new ResourceNotFoundException($"The planning with id {layer.Planning} doesn't exist");
                }

                if (!string.IsNullOrEmpty(planning.DeviceModelId))
                {
                    var devicesInLayer = await GetDevicesInLayerHierarchy(layer.Id);

                    var incompatibleDevices = devicesInLayer
                        .Where(d => d.DeviceModelId != planning.DeviceModelId)
                        .ToList();

                    if (incompatibleDevices.Any())
                    {
                        throw new IncompatibleDeviceModelException(
                            $"Cannot link layer '{layer.Name}' to planning. " +
                            $"The layer contains {incompatibleDevices.Count} device(s) with a different device model than required by the planning.");
                    }
                }
            }

            _ = this.mapper.Map(layer, layerEntity);

            this.layerRepository.Update(layerEntity);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get all devices in a layer and its child layers recursively
        /// </summary>
        private async Task<List<DeviceEntity>> GetDevicesInLayerHierarchy(string layerId)
        {
            var devices = new List<DeviceEntity>();

            // Get devices directly in this layer using filtered query
            var devicesInCurrentLayer = await this.deviceRepository.GetAllAsync(d => d.LayerId == layerId);
            devices.AddRange(devicesInCurrentLayer);

            // Get child layers using filtered query
            var childLayers = await this.layerRepository.GetAllAsync(l => l.Father == layerId);

            // Recursively get devices from child layers
            foreach (var childLayer in childLayers)
            {
                var childDevices = await GetDevicesInLayerHierarchy(childLayer.Id);
                devices.AddRange(childDevices);
            }

            return devices;
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
