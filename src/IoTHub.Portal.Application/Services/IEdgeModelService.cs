// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;

    public interface IEdgeModelService
    {
        Task<IEnumerable<IoTEdgeModelListItemDto>> GetEdgeModels(EdgeModelFilterDto edgeModelFilter);

        Task<IoTEdgeModelDto> GetEdgeModel(string modelId);

        Task CreateEdgeModel(IoTEdgeModelDto edgeModel);
        Task UpdateEdgeModel(IoTEdgeModelDto edgeModel);

        Task DeleteEdgeModel(string edgeModelId);

        Task<string> GetEdgeModelAvatar(string edgeModelId);

        Task<string> UpdateEdgeModelAvatar(string edgeModelId, IFormFile file);

        Task DeleteEdgeModelAvatar(string edgeModelId);

        Task<IEnumerable<IoTEdgeModuleDto>> GetPublicEdgeModules();
    }
}
