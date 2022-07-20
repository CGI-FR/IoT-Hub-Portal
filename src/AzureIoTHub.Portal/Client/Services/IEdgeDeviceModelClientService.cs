// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;

    public interface IEdgeDeviceModelClientService
    {
        Task<List<IoTEdgeModelListItem>> GetIoTEdgeModelList();

        Task<IoTEdgeModel> GetIoTEdgeModel(string modelId);

        Task<HttpResponseMessage> CreateIoTEdgeModel(IoTEdgeModel model);

        Task<HttpResponseMessage> UpdateIoTEdgeModel(IoTEdgeModel model);

        Task<HttpResponseMessage> DeleteIoTEdgeModel(string modelId);

        Task<string> GetAvatarUrl(string modelId);

        Task<HttpResponseMessage> ChangeAvatar(string id, MultipartFormDataContent content);

        Task<HttpResponseMessage> DeleteAvatar(string id);
    }
}
