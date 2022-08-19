// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;

    public interface IEdgeModelClientService
    {
        Task<List<IoTEdgeModelListItem>> GetIoTEdgeModelList();

        Task<IoTEdgeModel> GetIoTEdgeModel(string modelId);

        Task CreateIoTEdgeModel(IoTEdgeModel model);

        Task UpdateIoTEdgeModel(IoTEdgeModel model);

        Task DeleteIoTEdgeModel(string modelId);

        Task<string> GetAvatarUrl(string modelId);

        Task ChangeAvatar(string id, MultipartFormDataContent content);

        Task DeleteAvatar(string id);
    }
}
