// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.Filters;

    public interface IDeviceModelsClientService
    {
        Task<PaginationResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilter? deviceModelFilter = null);

        Task<DeviceModelDto> GetDeviceModel(string deviceModelId);

        Task<DeviceModelDto> CreateDeviceModel(DeviceModelDto deviceModel);

        Task UpdateDeviceModel(DeviceModelDto deviceModel);

        Task DeleteDeviceModel(string deviceModelId);

        Task<IList<DeviceProperty>> GetDeviceModelModelProperties(string deviceModelId);

        Task SetDeviceModelModelProperties(string deviceModelId, IList<DeviceProperty> deviceProperties);

        Task<string> GetAvatarUrl(string deviceModelId);

        Task ChangeAvatar(string deviceModelId, MultipartFormDataContent avatar);
    }
}
