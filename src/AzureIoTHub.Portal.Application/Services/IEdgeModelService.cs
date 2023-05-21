// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;

    public interface IEdgeModelService
    {
        Task<IEnumerable<IoTEdgeModelListItem>> GetEdgeModels(EdgeModelFilter edgeModelFilter);

        Task<IoTEdgeModel> GetEdgeModel(string modelId);

        Task CreateEdgeModel(IoTEdgeModel edgeModel);

        Task UpdateEdgeModel(IoTEdgeModel edgeModel);

        Task DeleteEdgeModel(string edgeModelId);

        Task<string> GetEdgeModelAvatar(string edgeModelId);

        Task<string> UpdateEdgeModelAvatar(string edgeModelId, IFormFile file);

        Task DeleteEdgeModelAvatar(string edgeModelId);

        Task SaveModuleCommands(IoTEdgeModel deviceModelObject);
    }
}
