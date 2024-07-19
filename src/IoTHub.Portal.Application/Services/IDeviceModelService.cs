// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;

    public interface IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        Task<PaginatedResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilter deviceModelFilter);

        Task<TModel> GetDeviceModel(string deviceModelId);

        Task<TModel> CreateDeviceModel(TModel deviceModel);

        Task UpdateDeviceModel(TModel deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<string> GetDeviceModelAvatar(string deviceModelId);

        Task<string> UpdateDeviceModelAvatar(string deviceModelId, IFormFile file);

        Task DeleteDeviceModelAvatar(string deviceModelId);
    }
}
