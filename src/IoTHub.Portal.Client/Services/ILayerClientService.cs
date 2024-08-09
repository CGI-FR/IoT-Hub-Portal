// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;

    public interface ILayerClientService
    {
        Task<string> CreateLayer(LayerDto layer);
        Task UpdateLayer(LayerDto layer);
        Task DeleteLayer(string modelId);
        Task<LayerDto> GetLayer(string layerId);
        Task<List<LayerDto>> GetLayers();
    }
}
