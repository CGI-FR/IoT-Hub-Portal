// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{

    public interface ILayerService
    {
        Task<LayerDto> CreateLayer(LayerDto level);
        Task UpdateLayer(LayerDto level);
        Task DeleteLayer(string levelId);
        Task<Layer> GetLayer(string levelId);
        Task<IEnumerable<LayerDto>> GetLayers();
    }
}
