// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.AspNetCore.Http;

    public interface IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        Task<IEnumerable<TListItem>> GetDeviceModels();

        Task<TModel> GetDeviceModel(string deviceModelId);

        Task CreateDeviceModel(TModel deviceModel);

        Task UpdateDeviceModel(TModel deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<string> GetDeviceModelAvatar(string deviceModelId);

        Task<string> UpdateDeviceModelAvatar(string deviceModelId, IFormFile file);

        Task DeleteDeviceModelAvatar(string deviceModelId);
    }
}
