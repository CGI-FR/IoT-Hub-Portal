// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{

    public interface ILayerService
    {
        Task<LayerDto> CreateLayer(LayerDto layer);
        Task UpdateLayer(LayerDto layer);
        Task DeleteLayer(string layerId);
        Task<Layer> GetLayer(string layerId);
        Task<IEnumerable<LayerDto>> GetLayers();
    }
}
