// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Portal.Models.v10;

    public interface IDeviceModelsClientService
    {
        Task<PaginationResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilterDto? deviceModelFilter = null);

        Task<DeviceModelDto> GetDeviceModel(string deviceModelId);

        Task<DeviceModelDto> CreateDeviceModel(DeviceModelDto deviceModel);

        Task UpdateDeviceModel(DeviceModelDto deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<IList<DevicePropertyDto>> GetDeviceModelModelProperties(string deviceModelId);

        Task SetDeviceModelModelProperties(string deviceModelId, IList<DevicePropertyDto> deviceProperties);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}
