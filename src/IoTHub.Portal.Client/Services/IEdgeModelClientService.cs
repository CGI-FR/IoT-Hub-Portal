// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.Filters;

    public interface IEdgeModelClientService
    {
        Task<List<IoTEdgeModelListItem>> GetIoTEdgeModelList(EdgeModelFilter? edgeModelFilter = null);

        Task<IoTEdgeModel> GetIoTEdgeModel(string modelId);

        Task CreateIoTEdgeModel(IoTEdgeModel model);

        Task UpdateIoTEdgeModel(IoTEdgeModel model);

        Task DeleteIoTEdgeModel(string modelId);

        Task<string> GetAvatarUrl(string modelId);

        Task ChangeAvatar(string id, MultipartFormDataContent content);

        Task DeleteAvatar(string id);

        Task<List<IoTEdgeModule>> GetPublicEdgeModules();
    }
}
